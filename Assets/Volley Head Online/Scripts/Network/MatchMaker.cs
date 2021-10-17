using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using UnityEngine.SceneManagement;

namespace VollyHead.Online
{
    [Serializable]
    public struct MatchInfo
    {
        public string matchId;
        public List<PlayerInfo> playerTeam1;
        public List<PlayerInfo> playerTeam2;
        public int players;
        public int maxPlayers;
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string playerName;
        public bool isRoomMaster;
        public int team;
        public bool ready;
        public string matchId;
    }

    public class MatchMaker : MonoBehaviour
    {
        public static MatchMaker instance;
        public event Action<NetworkConnection> OnPlayerDisconnected;

        public readonly Dictionary<NetworkConnection, Guid> playerMatches = new Dictionary<NetworkConnection, Guid>();

        // open room: able to join
        public readonly Dictionary<Guid, MatchInfo> openMatches = new Dictionary<Guid, MatchInfo>();

        // list player connection on specific match
        public readonly Dictionary<Guid, HashSet<NetworkConnection>> matchConnections = new Dictionary<Guid, HashSet<NetworkConnection>>();

        // all player infos from connection from client
        public readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();

        // list scene game match start
        public readonly Dictionary<Guid, Scene> matchStartScenes = new Dictionary<Guid, Scene>();
        
        // max player on match
        public int maxPlayerOnMatch = 4;

        // current joined match id
        public string currentClientMatchId = string.Empty;

        [Header("PLAYER DATA")]
        public PlayerInfo playerClientInfo;

        [Scene]
        public string gameScene = string.Empty;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        #region UI Functions

        internal void InitializeData()
        {
            playerMatches.Clear();
            openMatches.Clear();
            matchConnections.Clear();
            currentClientMatchId = string.Empty;
        }

        public void RequestCreateMatch()
        {
            if (!NetworkClient.active) return;

            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Create, playerInfo = this.playerClientInfo });
        }

        public void RequestJoinMatch(string matchId)
        {
            if (!NetworkClient.active || matchId == string.Empty) return;

            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Join, matchId = matchId.ToUpper(), playerInfo = this.playerClientInfo });
        }

        public void RequestLeaveMatch()
        {
            if (!NetworkClient.active || currentClientMatchId == string.Empty) return;

            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Leave, matchId = currentClientMatchId });
            Debug.Log($"Leave match...");
        }

        public void RequestCancelMatch()
        {
            if (!NetworkClient.active || currentClientMatchId == string.Empty) return;

            Debug.Log($"Cancel match...");
            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Cancel });
        }


        public void RequestChangeTeam()
        {
            if (!NetworkClient.active) return;

            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.ChangeTeam, matchId = currentClientMatchId });
        }
        public void RequestStartMatch()
        {
            if (!NetworkClient.active || currentClientMatchId == string.Empty) return;

            NetworkClient.connection.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Start });
        }

        #endregion

        #region Server Callbacks

        // Methods in this section are called from MatchNetworkManager's corresponding methods

        internal void OnStartServer()
        {
            if (!NetworkServer.active) return;

            Debug.Log("On Server Start!..");
            InitializeData();
            NetworkServer.RegisterHandler<ServerMatchMessage>(OnServerMatchMessage);
        }

        internal void OnServerReady(NetworkConnection conn)
        {
            if (!NetworkServer.active) return;

            Debug.Log("On Server Ready!..");

            playerInfos.Add(conn, new PlayerInfo { ready = false });
        }

        internal void OnServerDisconnect(NetworkConnection conn)
        {
            if (!NetworkServer.active) return;

            Debug.Log("On Server Disconnect!..");

            // Invoke OnPlayerDisconnected on all instances of MatchController
            OnPlayerDisconnected?.Invoke(conn);

            // if player disconnected is room master, delete match
            // else, leave match
            PlayerInfo playerInfo = playerInfos[conn];
            
            if (playerInfo.isRoomMaster)
            {
                OnServerCancelMatch(conn);
            }
            else
            {
                OnServerLeaveMatch(conn);
            }

            playerInfos.Remove(conn);
        }

        internal void OnStopServer()
        {
            InitializeData();
        }

        #endregion

        #region Client Callback

        internal void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("On Client Connect!..");
            playerInfos.Add(conn, new PlayerInfo { ready = false }); ;
        }

        internal void OnStartClient()
        {
            if (!NetworkClient.active) return;

            InitializeData();
            
            NetworkClient.RegisterHandler<ClientMatchMessage>(OnClientMatchMessage);
        }

        internal void OnClientDisconnect()
        {
            if (!NetworkClient.active) return;

            InitializeData();
        }

        internal void OnStopClient()
        {
            InitializeData();
            // reset canvas
            LobbyUIManager.instance.ResetLobby();
        }

        #endregion

        #region Server Match Message Handlers

        void OnServerMatchMessage(NetworkConnection conn, ServerMatchMessage msg)
        {
            if (!NetworkServer.active) return;

            switch (msg.serverMatchOperation)
            {
                case ServerMatchOperation.None:
                    {
                        Debug.LogWarning("Missing ServerMatchOperation");
                        break;
                    }
                case ServerMatchOperation.Create:
                    {
                        OnServerCreateMatch(conn, msg.playerInfo);
                        break;
                    }
                case ServerMatchOperation.Cancel:
                    {
                        OnServerCancelMatch(conn);
                        break;
                    }
                case ServerMatchOperation.Start:
                    {
                        OnServerStartMatch(conn);
                        break;
                    }
                case ServerMatchOperation.Join:
                    {
                        OnServerJoinMatch(conn, msg.matchId, msg.playerInfo);
                        break;
                    }
                case ServerMatchOperation.Leave:
                    {
                        OnServerLeaveMatch(conn);
                        break;
                    }
                case ServerMatchOperation.Ready:
                    {

                        break;
                    }
                case ServerMatchOperation.ChangeTeam:
                    {
                        OnServerChangeTeam(conn, msg.matchId);
                        break;
                    }
            }
        }

        void OnServerCreateMatch(NetworkConnection conn, PlayerInfo clientInfo)
        {
            if (!NetworkServer.active) return;

            // generate new match id
            string newMatchId;
            Guid newMatchGuid;
            do
            {
                newMatchId = MatchExtension.GenerateRandomMatchId();
                newMatchGuid = newMatchId.ToGuid();
            } while (matchConnections.ContainsKey(newMatchGuid));

            // add player to match connection list
            matchConnections.Add(newMatchGuid, new HashSet<NetworkConnection>());
            matchConnections[newMatchGuid].Add(conn);
            playerMatches.Add(conn, newMatchGuid);

            // set player infos
            PlayerInfo playerInfo = playerInfos[conn];
            playerInfo.playerName = clientInfo.playerName;
            playerInfo.isRoomMaster = true;
            playerInfo.ready = false;
            playerInfo.team = 1;
            playerInfo.matchId = newMatchId;
            playerInfos[conn] = playerInfo;

            // create new open match
            MatchInfo newMatch = new MatchInfo
            {
                matchId = newMatchId,
                maxPlayers = maxPlayerOnMatch,
                playerTeam1 = new List<PlayerInfo>(),
                playerTeam2 = new List<PlayerInfo>()
            };

            // add player to team 1 list
            newMatch.playerTeam1.Add(playerInfos[conn]);

            // add match info to open match list
            openMatches.Add(newMatchGuid, newMatch);

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Created, matchId = newMatchId, player = playerInfos[conn], playerTeam1 = newMatch.playerTeam1, playerTeam2 = newMatch.playerTeam2 });
        }

        void OnServerJoinMatch(NetworkConnection conn, string _matchId, PlayerInfo info)
        {
            // convert match id to Guid
            Guid matchGuid = _matchId.ToGuid();

            if (!NetworkServer.active || !matchConnections.ContainsKey(matchGuid) || !openMatches.ContainsKey(matchGuid)) return;
            if (openMatches[matchGuid].players >= openMatches[matchGuid].maxPlayers) return;

            // update player and match info
            PlayerInfo playerInfo = playerInfos[conn];
            MatchInfo matchInfo = openMatches[matchGuid];
            playerInfo.playerName = info.playerName;
            playerInfo.isRoomMaster = false;
            playerInfo.ready = false;
            playerInfo.matchId = _matchId;
            matchInfo.players++;
            if (matchInfo.playerTeam1.Count < 2)
            {
                playerInfo.team = 1;
                matchInfo.playerTeam1.Add(playerInfo);
            }
            else
            {
                playerInfo.team = 2;
                matchInfo.playerTeam2.Add(playerInfo);
            }
            playerInfos[conn] = playerInfo;
            openMatches[matchGuid] = matchInfo;
            matchConnections[matchGuid].Add(conn);

            Debug.Log($"{playerInfo.playerName} is join team {playerInfo.team}");

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Joined, player = playerInfos[conn], matchId = _matchId, playerTeam1 = matchInfo.playerTeam1, playerTeam2 = matchInfo.playerTeam2 });

            // update room another player on room 
            foreach (NetworkConnection playerConn in matchConnections[matchGuid])
            {
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, player = playerInfos[playerConn], playerTeam1 = matchInfo.playerTeam1, playerTeam2 = matchInfo.playerTeam2 });
            }

            Debug.Log("Join Success");
        }

        void OnServerChangeTeam(NetworkConnection conn, string matchId)
        {
            if (!NetworkServer.active) return;

            Guid matchGuid = matchId.ToGuid();
            PlayerInfo playerInfo = playerInfos[conn];
            MatchInfo matchInfo = openMatches[matchGuid];
            if (playerInfo.team == 1)
            {
                playerInfo.team = 2;
                matchInfo.playerTeam1.Remove(playerInfos[conn]);
                matchInfo.playerTeam2.Add(playerInfo);
            }
            else
            {
                playerInfo.team = 1;
                matchInfo.playerTeam2.Remove(playerInfos[conn]);
                matchInfo.playerTeam1.Add(playerInfo);
            }

            playerInfos[conn] = playerInfo;
            openMatches[matchGuid] = matchInfo;

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.ChangedTeam, player = playerInfos[conn] });

            foreach (NetworkConnection playerConn in matchConnections[matchGuid])
            {
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, player = playerInfos[conn], playerTeam1 = matchInfo.playerTeam1, playerTeam2 = matchInfo.playerTeam2 });
            }

            Debug.Log($"Change team success..");
        }

        void OnServerLeaveMatch(NetworkConnection conn)
        {
            if (!NetworkServer.active || currentClientMatchId == string.Empty) return;

            Debug.Log($"{playerInfos[conn].playerName} Leave match...");
            Guid matchGuid = currentClientMatchId.ToGuid();
            MatchInfo matchInfo = openMatches[matchGuid];
            PlayerInfo playerInfo = playerInfos[conn];

            // remove player info from list team match info
            if (playerInfo.team == 1)
            {
                matchInfo.playerTeam1.Remove(playerInfos[conn]);
            }
            else
            {
                matchInfo.playerTeam2.Remove(playerInfos[conn]);
            }
            
            // reset player info
            playerInfo.ready = false;
            playerInfo.matchId = string.Empty;
            playerInfo.team = 0;

            // substract player count on match info
            matchInfo.players--;

            playerInfos[conn] = playerInfo;
            openMatches[matchGuid] = matchInfo;

            // remove connection from match connection list
            foreach (KeyValuePair<Guid, HashSet<NetworkConnection>> kvp in matchConnections)
            {
                kvp.Value.Remove(conn);
            }

            // udpate room for another player on room
            foreach (NetworkConnection playerConn in matchConnections[matchGuid])
            {
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, player = playerInfos[playerConn], playerTeam1 = matchInfo.playerTeam1, playerTeam2 = matchInfo.playerTeam2 });
            }

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
        }

        void OnServerCancelMatch(NetworkConnection conn)
        {
            if (!NetworkServer.active || currentClientMatchId == string.Empty || !playerInfos[conn].isRoomMaster) return;

            Debug.Log($"Cancel match...");
            Guid matchGuid;

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Cancelled });

            if (playerMatches.TryGetValue(conn, out matchGuid))
            {
                playerMatches.Remove(conn);
                openMatches.Remove(matchGuid);

                foreach (NetworkConnection playerConn in matchConnections[matchGuid])
                {
                    PlayerInfo playerInfo = playerInfos[playerConn];
                    playerInfo.ready = false;
                    playerInfo.matchId = string.Empty;
                    playerInfos[playerConn] = playerInfo;
                    playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
                }
            }

        }

        void OnServerStartMatch(NetworkConnection conn)
        {
            if (!NetworkServer.active || !playerMatches.ContainsKey(conn)) return;

            Guid matchGuid;
            if (playerMatches.TryGetValue(conn, out matchGuid))
            {
                Debug.Log($"Start match: {matchGuid}");
                StartCoroutine(ServerLoadGameScene(conn, matchGuid));
            }
        }

        IEnumerator ServerLoadGameScene(NetworkConnection conn, Guid matchGuid)
        {
            yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics2D});

            Scene newMatchScene = SceneManager.GetSceneAt(matchStartScenes.Count + 1);
            matchStartScenes.Add(matchGuid, newMatchScene);

            foreach (NetworkConnection playerConn in matchConnections[matchGuid])
            {
                playerConn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Started });
                GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
#pragma warning disable 618
                player.GetComponent<NetworkMatch>().matchId = matchGuid;
#pragma warning restore 618
                NetworkServer.AddPlayerForConnection(playerConn, player);

                /* Reset ready state for after the match. */
                PlayerInfo playerInfo = playerInfos[playerConn];
                playerInfo.ready = false;
                playerInfos[playerConn] = playerInfo;

                SceneManager.MoveGameObjectToScene(playerConn.identity.gameObject, newMatchScene);
            }

            playerMatches.Remove(conn);
            openMatches.Remove(matchGuid);
            matchConnections.Remove(matchGuid);
        }
        #endregion

        #region Client Match Message Handler
        void OnClientMatchMessage(ClientMatchMessage msg)
        {
            if (!NetworkClient.active) return;

            switch (msg.clientMatchOperation)
            {
                case ClientMatchOperation.None:
                    {
                        Debug.LogWarning("Missing ClientMatchOperation");
                        break;
                    }
                case ClientMatchOperation.Created:
                    {
                        OnClientCreateMatch(msg.player, msg.playerTeam1, msg.playerTeam2, msg.matchId);
                        break;
                    }
                case ClientMatchOperation.Cancelled:
                    {
                        OnClientCancelled();
                        break;
                    }
                case ClientMatchOperation.Joined:
                    {
                        OnClientJoined(msg.player, msg.playerTeam1, msg.playerTeam2, msg.matchId);
                        break;
                    }
                case ClientMatchOperation.Departed:
                    {
                        OnClientDeparted();
                        break;
                    }
                case ClientMatchOperation.UpdateRoom:
                    {
                        OnClientUpdatedRoom(msg.playerTeam1, msg.playerTeam2);
                        break;
                    }
                case ClientMatchOperation.ChangedTeam:
                    {
                        OnClientChangedTeam(msg.player);
                        break;
                    }
                case ClientMatchOperation.Started:
                    {
                        OnClientMatchStarted();
                        break;
                    }
            }
        }

        void OnClientCreateMatch(PlayerInfo player, List<PlayerInfo> team1, List<PlayerInfo> team2, string _matchId)
        {
            currentClientMatchId = _matchId;
            playerClientInfo = player;

            // set room and match id ui
            LobbyUIManager.instance.ShowMatchRoom(_matchId, playerClientInfo.isRoomMaster);

            // update room
            LobbyUIManager.instance.UpdateRoom(team1, team2);
        }

        void OnClientJoined(PlayerInfo player, List<PlayerInfo> team1, List<PlayerInfo> team2, string _matchId)
        {
            currentClientMatchId = _matchId;
            playerClientInfo = player;

            // set room and match id ui
            LobbyUIManager.instance.ShowMatchRoom(_matchId, playerClientInfo.isRoomMaster);

            // update room
            LobbyUIManager.instance.UpdateRoom(team1, team2);
        }

        void OnClientDeparted()
        {
            currentClientMatchId = string.Empty;

            // back to lobby
            LobbyUIManager.instance.ResetLobby();
        }

        void OnClientCancelled()
        {
            currentClientMatchId = string.Empty;

            // back to lobby
            LobbyUIManager.instance.ResetLobby();
        }

        void OnClientChangedTeam(PlayerInfo playerChanged)
        {
            playerClientInfo = playerChanged;
        }

        void OnClientUpdatedRoom(List<PlayerInfo> team1, List<PlayerInfo> team2)
        {
            // update room player ui
            LobbyUIManager.instance.UpdateRoom(team1, team2);
        }

        void OnClientMatchStarted()
        {
            LobbyUIManager.instance.gameObject.SetActive(false);
        }

        #endregion
    }

    public static class MatchExtension
    {
        public static Guid ToGuid(this string _id)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(_id);
            byte[] hashBytes = provider.ComputeHash(inputBytes);

            return new Guid(hashBytes);
        }

        public static string GenerateRandomMatchId()
        {
            string _id = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                int random = UnityEngine.Random.Range(0, 36);
                if (random < 26)
                {
                    _id += (char)(random + 65);
                }
                else
                {
                    _id += (random - 26).ToString();
                }
            }

            Debug.Log($"Random Match ID: {_id}");
            return _id;
        }
    }
}