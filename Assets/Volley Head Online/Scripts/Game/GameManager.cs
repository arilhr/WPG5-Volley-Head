using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager instance;

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
        public float timeToNewRound;

        [Header("Team Info")]
        public Team[] teams;
        private Player currentPlayerToServe;

        [Header("Environment")]
        public GameObject ball;
        public Collider2D midBoundary;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetGameData(List<Player> playerTeam1, List<Player> playerTeam2, GameObject ballObject)
        {
            foreach (Player player in playerTeam1)
            {
                player.InitializeDataPlayer(0);
                teams[0].teamPlayer.Add(player);
            }

            foreach (Player player in playerTeam2)
            {
                player.InitializeDataPlayer(1);
                teams[1].teamPlayer.Add(player);
            }

            ball = ballObject;
        }

        public void StartGame()
        {
            RandomFirstTeamToServe();
            SetStartingPosition();
        }

        private void RandomFirstTeamToServe()
        {
            // random
            int teamService = UnityEngine.Random.Range(0, 2);
            RandomPlayerToServe(teamService);
        }

        private void RandomPlayerToServe(int _team)
        {
            int rand = UnityEngine.Random.Range(0, teams[_team].teamPlayer.Count);
            currentPlayerToServe = teams[_team].teamPlayer[rand];
        }

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

        public IEnumerator StartNewRound(int serviceTeam)
        {
            yield return new WaitForSeconds(timeToNewRound);

            RandomPlayerToServe(serviceTeam);
            SetStartingPosition();
            ball.GetComponent<Ball>().StartNewRound();
        }

        public void Scored(int scoredTeam)
        {
            // add score
            teams[scoredTeam].score += 1;

            // Score UI updated
            UIManager.instance.SetScore(teams[0].score, teams[1].score);

            // start a new round
            StartCoroutine(StartNewRound(scoredTeam));
        }
    }
}