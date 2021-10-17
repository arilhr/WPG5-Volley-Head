using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VollyHead.Online
{
    public class PlayerGUI : MonoBehaviour
    {
        public Image statusImg;
        public TMP_Text playerNameText;
        public bool isEmpty = true;
        
        public void ResetData()
        {
            playerNameText.text = string.Empty;
            isEmpty = true;
            SetCondition(false);
        }

        public void SetName(string _name)
        {
            playerNameText.text = $"{_name}";
        }

        public void SetCondition(bool isReady)
        {
            if (isReady)
            {
                statusImg.color = new Color32(85, 245, 85, 255);
            }
            else
            {
                statusImg.color = new Color32(221, 221, 221, 255);
            }
        }
    }
}