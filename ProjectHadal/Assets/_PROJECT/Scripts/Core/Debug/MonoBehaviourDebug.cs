//! Created by Jon
using UnityEngine;

namespace Hadal
{
    public class MonoBehaviourDebug : MonoBehaviour
    {
        protected bool allowDebug;

        protected void DoDebugEnabling(string keyCode)
        {
            if (DebugManager.Instance == null)
            {
                allowDebug = false;
                return;
            }
            allowDebug = DebugManager.Instance.EnableDebugging(keyCode);

            if (allowDebug) Debug.Log("Debug '" + keyCode + "' enabled.");
        }

        protected void DebugLog(object message)
        {
            if (allowDebug) Debug.Log(message);
        }

        protected void DebugWarning(object message)
        {
            if (allowDebug) Debug.LogWarning(message);
        }

        protected void DebugError(object message)
        {
            if (allowDebug) Debug.LogError(message);
        }
    }
}