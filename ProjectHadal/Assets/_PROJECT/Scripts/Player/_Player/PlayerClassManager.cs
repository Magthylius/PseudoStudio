using System;
using Hadal.Networking;
using Hadal.Networking.UI.MainMenu;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hadal.Player
{
    public class PlayerClassManager : MonoBehaviour
    {
        public static PlayerClassManager Instance;

        public PlayerClassData HarpoonerClass;
        public PlayerClassData SaviourClass;
        public PlayerClassData TrapperClass;
        public PlayerClassData InformerClass;
        
        [Space(10f)]
        [SerializeField, ReadOnly] private PlayerClassData currentPlayerClass;

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;

            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void Start()
        {
            MainMenuManager.Instance.ClassSelector.ClassChangedEvent += UpdateCurrentPlayerClass;
        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == NetworkEventManager.Instance.MainMenuScene)
            {
                MainMenuManager.Instance.ClassSelector.ClassChangedEvent += UpdateCurrentPlayerClass;
            }
        }

        [ContextMenu("Apply Class")]
        public void ApplyClass()
        {
            currentPlayerClass.SetUpUtility();
        }

        public void UpdateCurrentPlayerClass(PlayerClassType type)
        {
            switch (type)
            {
                case PlayerClassType.Harpooner: currentPlayerClass = HarpoonerClass; break;
                case PlayerClassType.Saviour: currentPlayerClass = SaviourClass; break;
                case PlayerClassType.Trapper: currentPlayerClass = TrapperClass; break;
                case PlayerClassType.Informer: currentPlayerClass = InformerClass; break;
            }
        }

        /*public void SetPlayerClass(PlayerClassData newPlayerClass)
        {
            PlayerClass = newPlayerClass;
        }*/
    }
}
