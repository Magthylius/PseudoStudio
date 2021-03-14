using UnityEngine;
using NaughtyAttributes;

namespace Hadal
{
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

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        public void ChangeGameState(GameState state) => currentGameState = state;
        public GameState CurrentGameState => currentGameState;
        public bool IsInMainMenu => currentGameState == GameState.IDLE ||
                                    currentGameState == GameState.WAITING;
        public bool IsInGame => currentGameState == GameState.ONGOING;

    }
}
