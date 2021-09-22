using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager instance;

        public float timeToNewRound = 2f;

        private List<Player> playerTeam1 = new List<Player>();
        private List<Player> playerTeam2 = new List<Player>();
        public Transform serviceAreaTeam1;
        public Transform serviceAreaTeam2;
        public Transform serviceBallTeam1;
        public Transform serviceBallTeam2;

        public GameObject ball;
        private GameObject spawnedBall;

        private int currentTeamService;
        private Player currentPlayerService;
        private int[] scoreTeam = new int[2];

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

        public void ResetVariable()
        {
            playerTeam1.Clear();
            playerTeam2.Clear();
        }

        [Server]
        public void StartGame()
        {
            // spawn ball
            spawnedBall = Instantiate(ball);
            NetworkServer.Spawn(spawnedBall);

            // reset score
            scoreTeam[0] = 0;
            scoreTeam[1] = 0;

            RandomFirstService();
            SetStartingPosition();

            Debug.Log("Game Started..");
        }

        /*
         * This is to random the team and player to service first time.
         */
        [Server]
        private void RandomFirstService()
        {
            // random
            currentTeamService = UnityEngine.Random.Range(0, 2);
            RandomPlayerToServe(currentTeamService);
        }

        /*
         * To random player on the team to serve
         */
        [Server]
        private void RandomPlayerToServe(int _team)
        {
            if (_team == 0)
            {
                int rand = UnityEngine.Random.Range(0, playerTeam1.Count);
                currentPlayerService = playerTeam1[rand];
            }
            else if (_team == 1)
            {
                int rand = UnityEngine.Random.Range(0, playerTeam2.Count);
                currentPlayerService = playerTeam2[rand];
            }
        }

        /* 
         This is to set the position of servicing player
         */
        [Server]
        private void SetStartingPosition()
        {
            if (currentTeamService == 0)
            {
                currentPlayerService.transform.position = serviceAreaTeam1.position;
                spawnedBall.transform.position = serviceBallTeam1.position;
            }
            else
            {
                currentPlayerService.transform.position = serviceAreaTeam2.position;
                spawnedBall.transform.position = serviceBallTeam2.position;
            }
        }

        [Server]
        public void StartNewRound(int serviceTeam)
        {
            RandomPlayerToServe(serviceTeam);
            SetStartingPosition();
        }

        [Server]
        public void AddPlayer(int _teamID, GameObject _player)
        {
            Player p = _player.GetComponent<Player>();
            if (_teamID == 0) playerTeam1.Add(p);
            else playerTeam2.Add(p);
        }

        public void AddScore(int _team)
        {
            scoreTeam[_team] += 1;

        }
    }
}

