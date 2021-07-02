using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
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
        [ReadOnly] GameState currentGameState;
        public List<PhotonView> pViewList;

        public event GameEvent GameStartedEvent;
        public event GameEvent GameEndedEvent;
        public event GameEvent SceneLoadedEvent;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;

            SceneManager.sceneLoaded += LoadSceneEvent;
        }
        
        void StartEndScreenAndReturn()
        {
            Debug.LogWarning("Game ended!");
        }

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
