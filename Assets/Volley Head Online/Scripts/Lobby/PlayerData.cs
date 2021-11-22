using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VollyHead.Online
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData instance;

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
                // TODO: Lobby UI Create Name
            }
        }

        public void SetName(string newName)
        {
            playerName = newName;
        }
    }
}