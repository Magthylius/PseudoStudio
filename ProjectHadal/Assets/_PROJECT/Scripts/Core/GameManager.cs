using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using System.Collections.Generic;

namespace Hadal
{
    public delegate void GameEvent();
    
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

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        public void StartGameEvent() => GameStartedEvent?.Invoke();
        public void EndGameEvent() => GameEndedEvent?.Invoke();
        
        public void ChangeGameState(GameState state) => currentGameState = state;
        public GameState CurrentGameState => currentGameState;
        public bool IsInMainMenu => currentGameState == GameState.IDLE ||
                                    currentGameState == GameState.WAITING;
        public bool IsInGame => currentGameState == GameState.ONGOING;

    }
}
