using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VollyHead.Online
{
    public class AutoConnect : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            if (!Application.isBatchMode)
            { //Headless build
                Debug.Log($"=== Client Build ===");
                NetworkManager.singleton.StartClient();
            }
            else
            {
                Debug.Log($"=== Server Build ===");
            }

        }
    }
}