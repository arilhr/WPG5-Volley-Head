using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VollyHead.Online
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData instance;

        public GameObject enterNameUI;
        public TMP_InputField nameInput;
        public string playerName;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerName == string.Empty)
            {
                ShowEnterName();
            }
        }

        #region UI Function
        public void EnterName()
        {
            SetName(nameInput.text);
            enterNameUI.SetActive(false);
        }
        #endregion

        private void ShowEnterName()
        {
            enterNameUI.SetActive(true);
        }

        public void SetName(string newName)
        {
            playerName = newName;
        }
    }
}