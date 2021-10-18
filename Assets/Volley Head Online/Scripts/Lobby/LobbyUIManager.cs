using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VollyHead.Online
{
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager instance;

        public GameObject mainMenu;
        public GameObject joinRoomPanel;
        public TMP_InputField roomCodeInput;
        public GameObject roomPanel;
        public RoomGUI roomController;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void CreateMatch()
        {
            MatchMaker.instance.RequestCreateMatch();
        }

        public void JoinMatch()
        {
            if (roomCodeInput.text == string.Empty) return;

            MatchMaker.instance.RequestJoinMatch(roomCodeInput.text.ToUpper());
        }

        public void ChangeTeam()
        {
            MatchMaker.instance.RequestChangeTeam();
        }

        public void LeaveMatch()
        {
            if (MatchMaker.instance.playerClientInfo.isRoomMaster)
            {
                MatchMaker.instance.RequestCancelMatch();
                Debug.Log($"Request cancel match...");
            }
            else
            {
                MatchMaker.instance.RequestLeaveMatch();
                Debug.Log($"Request leave match...");
            }
                
        }

        public void StartMatch()
        {
            if (!MatchMaker.instance.playerClientInfo.isRoomMaster) return;

            MatchMaker.instance.RequestStartMatch();
        }
        
        public void ShowMatchRoom(string matchId, bool isRoomMaster)
        {
            // show room panel
            mainMenu.SetActive(false);
            roomPanel.SetActive(true);

            // match id
            roomController.SetRoom(matchId, isRoomMaster);
        }

        public void UpdateRoom(int playerInRoom, List<PlayerInfo> team1, List<PlayerInfo> team2)
        {
            roomController.ResetRoom();

            if (MatchMaker.instance.playerClientInfo.team == 1 && team2.Count < 2) roomController.changeTeamBtn.SetActive(true);
            if (MatchMaker.instance.playerClientInfo.team == 2 && team1.Count < 2) roomController.changeTeamBtn.SetActive(true);

            foreach (PlayerInfo player in team1)
            {
                PlayerGUI emptySlot = roomController.teams1GUI.Find(x => x.isEmpty);
                emptySlot.SetName(player.playerName);
                emptySlot.SetCondition(player.ready);
                emptySlot.isEmpty = false;
            }

            foreach (PlayerInfo player in team2)
            {
                PlayerGUI emptySlot = roomController.teams2GUI.Find(x => x.isEmpty);
                emptySlot.SetName(player.playerName);
                emptySlot.SetCondition(player.ready);
                emptySlot.isEmpty = false;
            }

            if (playerInRoom >= MatchMaker.instance.minPlayerToStart)
            {
                roomController.startBtn.SetActive(true);
            }
            else
            {
                roomController.startBtn.SetActive(false);
            }
        }

        public void ResetLobby()
        {
            mainMenu.SetActive(true);
            joinRoomPanel.SetActive(false);
            roomPanel.SetActive(false);
            roomController.ResetRoom();
        }
    }
}