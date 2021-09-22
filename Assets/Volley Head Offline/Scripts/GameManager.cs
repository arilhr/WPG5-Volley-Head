using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VollyHead.Offline
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public float timeToNewRound = 2f;

        public Player[] playerTeam1;
        public Player[] playerTeam2;
        public GameObject ball;

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

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            // reset score
            scoreTeam[0] = 0;
            scoreTeam[1] = 0;

            RandomFirstService();
            SetStartingPosition();
        }

        /*
         * This is to random the team and player to service first time.
         */
        private void RandomFirstService()
        {
            // random
            int teamService = UnityEngine.Random.Range(0, 2);
            RandomPlayerToServe(teamService);
        }

        /*
         * To random player on the team to serve
         */
        private void RandomPlayerToServe(int _team)
        {
            if (_team == 0)
            {
                int rand = UnityEngine.Random.Range(0, playerTeam1.Length);
                currentPlayerService = playerTeam1[rand];
            }
            else if (_team == 1)
            {
                int rand = UnityEngine.Random.Range(0, playerTeam2.Length);
                currentPlayerService = playerTeam2[rand];
            }
        }

        /* 
         This is to sset the position of servicing player
         */
        private void SetStartingPosition()
        {
            currentPlayerService.transform.position = currentPlayerService.serviceArea.position;
            ball.transform.position = currentPlayerService.serviceBallPos.transform.position;
        }

        public void StartNewRound(int serviceTeam)
        {
            RandomPlayerToServe(serviceTeam);
            SetStartingPosition();
        }

        public void AddScore(int _team)
        {
            scoreTeam[_team] += 1;

            // Score UI updated
            UIManager.instance.teamScoreText[_team].text = scoreTeam[_team].ToString();
        }
    }

}
