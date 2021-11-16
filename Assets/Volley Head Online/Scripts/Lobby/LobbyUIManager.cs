using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VollyHead.Online
{
    public class LobbyUIManager : MonoBehaviour
    {
        public static LobbyUIManager instance;

        [Header("MAIN MENU UI")]
        public GameObject mainMenu;
        [Space(5f)]

        [Header("JOIN ROOM UI")]
        public GameObject joinRoomPanel;
        public TMP_InputField roomCodeInput;

        [Header("ROOM")]
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
            MatchMaker.instance.RequestLeaveMatch();
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

        public void UpdateRoom(MatchInfo matchInfo)
        {
            roomController.ResetRoom();

            if (MatchMaker.instance.playerClientInfo.team == 1 && matchInfo.playerTeam2.Count < 2) roomController.changeTeamBtn.SetActive(true);
            if (MatchMaker.instance.playerClientInfo.team == 2 && matchInfo.playerTeam1.Count < 2) roomController.changeTeamBtn.SetActive(true);

            foreach (PlayerInfo player in matchInfo.playerTeam1)
            {
                PlayerGUI emptySlot = roomController.teams1GUI.Find(x => x.isEmpty);
                emptySlot.SetPlayerUI(player.playerName, player.isRoomMaster);
                emptySlot.isEmpty = false;
            }

            foreach (PlayerInfo player in matchInfo.playerTeam2)
            {
                PlayerGUI emptySlot = roomController.teams2GUI.Find(x => x.isEmpty);
                emptySlot.SetPlayerUI(player.playerName, player.isRoomMaster);
                emptySlot.isEmpty = false;
            }

            if (matchInfo.playersCount >= MatchMaker.instance.minPlayerToStart && MatchMaker.instance.playerClientInfo.isRoomMaster)
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