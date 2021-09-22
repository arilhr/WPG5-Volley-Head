using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VollyHead.Online
{
    public class NetworkManagerVolley : NetworkManager
    {
        public Transform[] startingPosTeam1 = new Transform[2];
        public Transform[] startingPosTeam2 = new Transform[2];

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at spawn position
            GameObject player = null;
            int teamId = numPlayers % 2 == 0 ? 0 : 1;
            int playerNum = numPlayers < 2 ? 0 : 1;

            if (teamId == 0)
            {
                Transform spawnPos = startingPosTeam1[playerNum];
                player = Instantiate(playerPrefab, spawnPos.position, Quaternion.identity);
            }
            else
            {
                Transform spawnPos = startingPosTeam2[playerNum];
                player = Instantiate(playerPrefab, spawnPos.position, Quaternion.identity);
            }

            NetworkServer.AddPlayerForConnection(conn, player);
            GameManager.instance.AddPlayer(teamId, player);

            if (numPlayers == 2)
            {
                GameManager.instance.StartGame();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            GameManager.instance.ResetVariable();
        }
    }
}

