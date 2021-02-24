using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    public class PersistenceManager : MonoBehaviourDebug
    {
        public string debugKey;
        public static PersistenceManager Instance;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                DebugWarning("Persistence Manager group destroyed.");
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            DoDebugEnabling(debugKey);
        }

    }
}
