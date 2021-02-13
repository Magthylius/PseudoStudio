//! Created by Jon
using UnityEngine;

namespace Hadal
{
    public class MonoBehaviourDebug : MonoBehaviour
    {
        bool allowDebug;

        protected void DoDebugEnabling(string keyCode)
        {
            allowDebug = DebugManager.Instance.EnableDebugging(keyCode);
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