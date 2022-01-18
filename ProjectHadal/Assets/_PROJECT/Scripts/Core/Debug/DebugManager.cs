using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;

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

        [System.Serializable]
        public struct ScreenLogger
        {
            public TextMeshProUGUI tmp;
            public ScreenLogger(GameObject gameObject) => tmp = gameObject.GetComponent<TextMeshProUGUI>();
            public void Log(string text) => tmp.text = text;
        }

        public static DebugManager Instance;

        [Header("Debug Log Settings")]
        [SerializeField] bool debugLoggingEnabled = true;

        [Header("Screen Log Settings")]
        [SerializeField] bool screenLoggingEnabled = true;
        public GameObject screenLoggerPrefab;
        public GameObject screenLoggerCanvas;
        public Transform screenLoggerParent;

        [Header("Key List")]
        public List<DebugKey> debugKeyList;

        [Header("Logger List")]
        [ReadOnly] public List<ScreenLogger> ScreenLoggers;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            ScreenLoggers = new List<ScreenLogger>();
            DetermineLogs();
        }

        public bool EnableDebugging(string keyCode)
        {
            foreach (DebugKey debugKey in debugKeyList)
            {
                if (debugKey.key == keyCode) return debugKey.allowDebug;
            }

            return false;
        }

        #region Screen Logging
        /// <summary>
        /// Creates an instance of screen logger.
        /// </summary>
        /// <returns>Index of screen logger</returns>
        public int CreateScreenLogger()
        {
            GameObject logger = Instantiate(screenLoggerPrefab, screenLoggerParent);
            ScreenLoggers.Add(new ScreenLogger(logger));
            return ScreenLoggers.Count - 1;
        }
        /// <summary>
        /// Updates screen logger with given text.
        /// </summary>
        /// <param name="index">Index of logger.</param>
        /// <param name="update">Updated text.</param>
        public void SLog(int index, object update) => ScreenLoggers[index].Log(update.ToString());
        /// <summary>
        /// Updates screen logger with given text.
        /// </summary>
        /// <param name="index">Index of logger.</param>
        /// <param name="prefix">Prefix before update.</param>
        /// <param name="update">Updated text.</param>
        public void SLog(int index, string prefix, object update) => ScreenLoggers[index].Log(prefix + ": " + update.ToString()); 
        #endregion

        #region Accessors
        public void SetDebugLogState(bool state)
        {
            debugLoggingEnabled = state;
            DetermineLogs();
        }
        public void SetScreenLogState(bool state)
        {
            screenLoggingEnabled = state;
            DetermineLogs();
        }
        void DetermineLogs()
        {
#if UNITY_EDITOR
            if (debugLoggingEnabled) Debug.unityLogger.logEnabled = true;
            else Debug.unityLogger.logEnabled = false;
#else
                Debug.unityLogger.logEnabled = false;
#endif
            if (!screenLoggingEnabled) screenLoggerCanvas.SetActive(false);
        } 
        #endregion
    }
}
