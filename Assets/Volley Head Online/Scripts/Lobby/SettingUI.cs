using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VollyHead.Online
{
    public class SettingUI : MonoBehaviour
    {
        public TMP_Text playerNameText;

        private void Update()
        {
            SetPlayerNameUI();
        }

        private void SetPlayerNameUI()
        {
            playerNameText.text = $"{PlayerData.instance.playerName}";
        }
    }
}