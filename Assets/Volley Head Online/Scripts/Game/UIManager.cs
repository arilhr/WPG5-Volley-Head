using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace VollyHead.Online
{
    public class UIManager : NetworkBehaviour
    {
        public static UIManager instance;

        public TMP_Text[] teamScoreText;

        [Header("Serve UI")]
        public GameObject serveUI;
        public Slider serveBar;
        public CustomButton serveButton;

        [Header("Game Button")]
        public GameObject movementUI;
        public CustomButton leftButton;
        public CustomButton rightButton;


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

        public void SetServeUI()
        {
            serveBar.value = 0;
            serveUI.SetActive(true);
            movementUI.SetActive(false);
        }

        public void SetServePowerUI(float power)
        {
            serveBar.value = power;
        }

        public void SetMoveUI()
        {
            serveUI.SetActive(false);
            movementUI.SetActive(true);
        }

        [ClientRpc]
        public void SetScore(int team1, int team2)
        {
            teamScoreText[0].text = team1.ToString();
            teamScoreText[1].text = team2.ToString();
        }

        
    }
}