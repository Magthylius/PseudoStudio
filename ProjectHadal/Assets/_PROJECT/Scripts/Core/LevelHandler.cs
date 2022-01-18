using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hadal
{
    public class LevelHandler : MonoBehaviour
    {
        [System.Serializable]
        public class LevelSettings
        {
            [Scene] public string Scene;
            public bool BypassObjectPoolingChecks;
        }

        public List<LevelSettings> LevelSettingsList;

        public LevelSettings GetLevelSettings(string sceneName)
        {
            return LevelSettingsList.Single(level => level.Scene == sceneName);
        }

        public LevelSettings GetCurrentSceneSettings()
        {
            return GetLevelSettings(SceneManager.GetActiveScene().name);
        }

        public string CurrentScene => SceneManager.GetActiveScene().name;
    }
}
