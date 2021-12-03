using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class GameManager : NetworkBehaviour
    {
        [Serializable]
        public struct Team
        {
            [HideInInspector] public int score;
            public List<Player> teamPlayer;
            public List<Transform> startingPos;
            public Transform serveArea;
            public Transform ballPosOnServe;
        }

        [Header("Game")]
        public UIManager gameUI;
        public int targetScore = 5;
        public float timeToNewRound;

        [Header("Team Info")]
        public Team[] teams;
        private Player currentPlayerToServe;

        [Header("Environment")]
        public Ball ball;
        public Collider2D midBoundary;

        [Server]
        public void InitializeGameData(List<Player> playerTeam1, List<Player> playerTeam2, Ball _ball)
        {
            foreach (Player player in playerTeam1)
            {
                player.InitializeDataServerPlayer(this, 0);
                teams[0].teamPlayer.Add(player);
            }

            foreach (Player player in playerTeam2)
            {
                player.InitializeDataServerPlayer(this, 1);
                teams[1].teamPlayer.Add(player);
            }

            ball = _ball;
            ball.InitializeBallData(this);
        }

        [Server]
        public void StartGame()
        {
            RandomFirstTeamToServe();
            SetStartingPosition();
        }

        [Server]
        private void RandomFirstTeamToServe()
        {
            // random
            int teamService = UnityEngine.Random.Range(0, 2);
            RandomPlayerToServe(teamService);
        }

        [Server]
        private void RandomPlayerToServe(int _team)
        {
            int rand = UnityEngine.Random.Range(0, teams[_team].teamPlayer.Count);
            currentPlayerToServe = teams[_team].teamPlayer[rand];
        }

        [Server]
        private void SetStartingPosition()
        {
            // set player starting pos
            currentPlayerToServe.transform.position = teams[currentPlayerToServe.GetTeam()].serveArea.position;
            ball.transform.position = teams[currentPlayerToServe.GetTeam()].ballPosOnServe.position;
            currentPlayerToServe.StartServe();

            // set another player position
            foreach (Team team in teams)
            {
                int index = 0;
                foreach (Player player in team.teamPlayer)
                {
                    if (player != currentPlayerToServe)
                    {
                        player.transform.position = team.startingPos[index].position;
                        player.StartMove();
                        index++;
                    }
                }
            }

            ball.GetComponent<Ball>().ServeMode();
        }

        [Server]
        public IEnumerator StartNewRound(int serviceTeam)
        {
            yield return new WaitForSeconds(timeToNewRound);

            RandomPlayerToServe(serviceTeam);
            SetStartingPosition();
            ball.GetComponent<Ball>().StartNewRound();
        }

        [Server]
        public void Scored(int scoredTeam)
        {
            // add score
            teams[scoredTeam].score += 1;

            // scored ui
            RpcScored(teams[0].score, teams[1].score);

            if (CheckWin(scoredTeam)) return;

            // start a new round
            StartCoroutine(StartNewRound(scoredTeam));
        }

        [ClientRpc]
        private void RpcScored(int team1Score, int team2Score)
        {
            Debug.Log($"{team1Score} || {team2Score}");

            // Score UI updated
            gameUI.SetScore(team1Score, team2Score);
        }

        [Server]
        private bool CheckWin(int scoredTeam)
        {
            if (teams[scoredTeam].score == targetScore)
            {
                GameEnd(scoredTeam);
                return true;
            }

            return false;
        }

        [Server]
        private void GameEnd(int winnerTeam)
        {
            int loserTeam = winnerTeam == 0 ? 1 : 0;

            foreach (Player player in teams[winnerTeam].teamPlayer)
            {
                RpcGameWin(player.connectionToClient);
            }

            foreach (Player player in teams[loserTeam].teamPlayer)
            {
                RpcGameLose(player.connectionToClient);
            }
        }

        [TargetRpc]
        private void RpcGameLose(NetworkConnection target)
        {
            gameUI.SetGameEndUI(false);
            Debug.Log($"Lose...");
        }

        [TargetRpc]
        private void RpcGameWin(NetworkConnection target)
        {
            gameUI.SetGameEndUI(true);
            Debug.Log($"Win...");
        }

        public void OnPlayerDisconnected(NetworkConnection conn)
        {
            string matchId = MatchMaker.instance.playerInfos[conn].matchId;
            StartCoroutine(ServerEndMatch(conn, matchId));
        }

        // match disconnected
        public IEnumerator ServerEndMatch(NetworkConnection conn, string matchId)
        {
            MatchMaker.instance.OnPlayerDisconnected -= OnPlayerDisconnected;

            NetworkServer.Destroy(ball.gameObject);

            // Skip a frame so the message goes out ahead of object destruction
            yield return null;

            foreach (NetworkConnection playerConn in MatchMaker.instance.matchConnections[matchId.ToGuid()])
            {
                if (playerConn != conn)
                {
                    NetworkServer.RemovePlayerForConnection(playerConn, true);
                }
            }

            StartCoroutine(GameManagerTimeoutEndMatch(matchId));
        }

        private IEnumerator GameManagerTimeoutEndMatch(string matchId)
        {
            yield return new WaitForSeconds(5f);

            if (MatchMaker.instance.matchConnections.ContainsKey(matchId.ToGuid()))
            {
                if (MatchMaker.instance.matchConnections[matchId.ToGuid()].Count > 0)
                {
                    foreach (NetworkConnection playerConn in MatchMaker.instance.matchConnections[matchId.ToGuid()])
                    {
                        RpcExitGame(playerConn);
                    }
                }
            }

            NetworkServer.Destroy(gameObject);
            MatchMaker.instance.OnServerMatchEnded(matchId);
        }

        [TargetRpc]
        private void RpcExitGame(NetworkConnection conn)
        {
            LobbyUIManager.instance.ResetLobby();
        }

        [Client]
        public void BackToMenu()
        {
            LobbyUIManager.instance.ResetLobby();
            CmdBackMenu();

            Destroy(gameObject);
        }

        [Command]
        private void CmdBackMenu(NetworkConnectionToClient conn = null)
        {
            MatchMaker.instance.RemovePlayerFromMatch(conn, GetComponent<NetworkMatch>().matchId);
        }
    }
}