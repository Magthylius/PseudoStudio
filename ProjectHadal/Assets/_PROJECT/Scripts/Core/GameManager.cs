using System;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Hadal
{
    public delegate void GameEvent(bool booleanData);

    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            IDLE,
            WAITING,
            ONGOING
        }

        public static GameManager Instance;

        [Header("Settings")] 
        [Scene] public string MainMenuScene;
        [Scene] public string InGameScene;
        
        [Header("Data")]
        [ReadOnly] GameState currentGameState;
        [ReadOnly] public List<PhotonView> pViewList;
        [ReadOnly, SerializeField] private bool enableLevelTimer = false;
        [ReadOnly, SerializeField] private float levelTimer = 0f;

        public event GameEvent GameStartedEvent;
        public event GameEvent GameEndedEvent;
        public event GameEvent SceneLoadedEvent;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += LoadSceneEvent;
            SceneManager.sceneLoaded += HandleSceneLoad;
            GameEndedEvent += HandleGameEndedEvent;
        }

        private void FixedUpdate()
        {
            if (enableLevelTimer)
            {
                levelTimer += Time.fixedDeltaTime;
            }
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= LoadSceneEvent;
            SceneManager.sceneLoaded -= HandleSceneLoad;
            GameEndedEvent -= HandleGameEndedEvent;
        }

        void HandleSceneLoad(Scene scene, LoadSceneMode mode)
        {
            ResetTimer();
     
            if (scene.name == MainMenuScene)
                DisableTimer();
            else if (scene.name == InGameScene)
                EnableTimer();
            
            StaticClassManager.Instance.CallForReset();
        }

        void HandleGameEndedEvent(bool playersWon)
        {
            DisableTimer();
        }
        
        public void EnableTimer() => enableLevelTimer = true;
        public void DisableTimer() => enableLevelTimer = false;
        public void ResetTimer() => levelTimer = 0f;
        public float LevelTimer => levelTimer;

        public void StartGameEvent() => GameStartedEvent?.Invoke(false);
        public void EndGameEvent(bool playersWon) => GameEndedEvent?.Invoke(playersWon);
        void LoadSceneEvent(Scene scene, LoadSceneMode mode) => SceneLoadedEvent?.Invoke(false);

        public void ChangeGameState(GameState state) => currentGameState = state;
        public GameState CurrentGameState => currentGameState;
        public bool IsInMainMenu => currentGameState == GameState.IDLE ||
                                    currentGameState == GameState.WAITING;
        public bool IsInGame => currentGameState == GameState.ONGOING;

    }
}
