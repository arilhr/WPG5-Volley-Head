using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VollyHead.Offline
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        public Text[] teamScoreText;

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
    }

}
