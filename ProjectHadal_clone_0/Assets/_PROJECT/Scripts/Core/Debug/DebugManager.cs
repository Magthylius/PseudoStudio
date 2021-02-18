using UnityEngine;

//! Created by Jon
namespace Hadal
{
    public class DebugManager : MonoBehaviour
    {
        [System.Serializable]
        public struct DebugKey
        {
            public string key;
            public bool allowDebug;
        }

        public static DebugManager Instance;

        public bool debugEnabled = true;
        public System.Collections.Generic.List<DebugKey> debugKeyList;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            #if UNITY_EDITOR
                if (debugEnabled) Debug.unityLogger.logEnabled = true;
                else Debug.unityLogger.logEnabled = false;
            #else
                Debug.unityLogger.logEnabled = false;
            #endif
        }

        public bool EnableDebugging(string keyCode)
        {
            foreach (DebugKey debugKey in debugKeyList)
            {
                if (debugKey.key == keyCode) return debugKey.allowDebug;
            }

            return false;
        }
    }
}
