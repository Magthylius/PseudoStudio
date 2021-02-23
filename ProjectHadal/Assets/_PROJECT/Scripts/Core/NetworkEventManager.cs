using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Hadal
{
    public class NetworkEventManager : MonoBehaviourPunCallbacks
    {
        public static NetworkEventManager Instance;

        #region Byte Declarations
        #endregion

        void Awake()
        {
            if (Instance != null)
            {
                gameObject.name += " (Deprecated)";
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
    }
}
