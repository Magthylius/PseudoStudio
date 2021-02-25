using UnityEngine;
using NaughtyAttributes;

namespace Hadal
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            MAIN_MENU_LOBBY,
            MAIN_MENU_ROOM,
            MAIN_MENU_CONNECTING,
            IN_GAME_PREPARATION,
            IN_GAME_HUNTING
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
        public bool IsInMainMenu => currentGameState == GameState.MAIN_MENU_LOBBY ||
                                    currentGameState == GameState.MAIN_MENU_ROOM ||
                                    currentGameState == GameState.MAIN_MENU_CONNECTING;
        public bool IsInGame => currentGameState == GameState.IN_GAME_PREPARATION ||
                                currentGameState == GameState.IN_GAME_HUNTING;

    }
}
