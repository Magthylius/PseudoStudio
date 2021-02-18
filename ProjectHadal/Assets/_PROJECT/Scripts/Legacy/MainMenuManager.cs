using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Magthylius.LerpFunctions;
using UnityEngine.SceneManagement;

//! C: Jon
namespace Hadal.Legacy
{
    public class MainMenuManager : MenuManager
    {
        public new static MainMenuManager Instance;

        enum MenuPhase
        {
            START = 0,
            MAIN
        }

        MenuPhase menuPhase = MenuPhase.START;
        [SerializeField] Launcher launcher;

        [Header("Menu settings")]
        [SerializeField] Menu startMenu;
        [SerializeField] Menu nicknameMenu;
        [SerializeField] Menu lobbyMenu;
        [SerializeField] Menu loadingMenu;
        [SerializeField] Menu roomMenu;

        [Header("Start settings")]
        [SerializeField] Image startFiller;
        [Range(0f, 1f)] public float startFillerSpeed = 0.5f;

        [Space(10f)]
        [SerializeField] TextMeshProUGUI versionTMP;

        ImageFiller startIF;

        [Header("Nickname settings")]
        [SerializeField] TMP_InputField nicknameTMPInput;
        [SerializeField] TextMeshProUGUI lobbyNicknameTMP;

        [Header("Lobby settings")]
        [SerializeField] Menu gameOptions;
        [SerializeField] Menu roomOptions;

        [Header("Room Joining settings")]
        [Min(0f)] public float roomPanelLerpSpeed;
        [SerializeField] RectTransform createRoomPanel;
        [SerializeField] RectTransform findRoomPanel;
        [SerializeField] TMP_InputField createRoomTMPInput;

        FlexibleRect createRoomFR;
        FlexibleRect findRoomFR;

        [Header("Room Ready settings")]
        public GameObject startGameButton;
        [SerializeField] string nextLevelName;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            versionTMP.text = "V " + Application.version;

            startIF = new ImageFiller(startFiller, startFillerSpeed, 1f);
            startIF.OnFillComplete += EndStartPhase;

            createRoomFR = new FlexibleRect(createRoomPanel);
            createRoomFR.SetTargetPosition(createRoomFR.GetBodyOffset(Vector2.right));
            createRoomFR.MoveToEnd();

            findRoomFR = new FlexibleRect(findRoomPanel);
            findRoomFR.SetTargetPosition(findRoomFR.GetBodyOffset(Vector2.right));
            findRoomFR.MoveToEnd();

            OpenMenu(startMenu);
            OpenMenu(gameOptions);
            CloseMenu(nicknameMenu);
            CloseMenu(lobbyMenu);
            CloseMenu(roomOptions);
            CloseMenu(loadingMenu);
        }

        void Update()
        {
            switch (menuPhase)
            {
                case MenuPhase.START:
                    startIF.Step(Time.unscaledDeltaTime);
                    break;
                case MenuPhase.MAIN:
                    createRoomFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    findRoomFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    break;
            }
        }

        #region Main Menu 
        void ChangePhase(MenuPhase phase) => menuPhase = phase;

        #region Start phase
        public void PNTR_ChargeStartFiller() => startIF.StartCharge();
        public void PNTR_DischargeStartFiller() => startIF.StopCharge();
        void EndStartPhase()
        {
            ChangePhase(MenuPhase.MAIN);
            CloseMenu(startMenu);

            if (PlayerPrefs.HasKey("PlayerName"))
            {
                UpdateLobbyNickname();
                OpenMenu(lobbyMenu);
            }
            else
            {
                OpenMenu(nicknameMenu);
            }
        }
        #endregion

        #region Nickname phase
        public void BTN_ApplyName()
        {
            PlayerPrefs.SetString("PlayerName", nicknameTMPInput.text);
            UpdateLobbyNickname();

            CloseMenu(nicknameMenu);
            OpenMenu(lobbyMenu);
        }

        void UpdateLobbyNickname()
        {
            lobbyNicknameTMP.text = PlayerPrefs.GetString("PlayerName").ToUpper();
            launcher.ChangeNickname(PlayerPrefs.GetString("PlayerName"));
        }
        #endregion

        #region Lobby Phase
        public void BTN_StartGame()
        {
            CloseMenu(gameOptions);
            OpenMenu(roomOptions);
        }

        public void BTN_BackToLobby()
        {
            CloseMenu(roomOptions);
            OpenMenu(gameOptions);

            findRoomFR.StartLerp(true);
            createRoomFR.StartLerp(true);
        }

        public void BTN_FindRooms()
        {
            findRoomFR.StartLerp(false);
            createRoomFR.StartLerp(true);
        }

        public void BTN_CreateRooms()
        {
            createRoomFR.StartLerp(false);
            findRoomFR.StartLerp(true);
        }

        public void BTN_CreateActualRoom()
        {
            launcher.CreateRoom(createRoomTMPInput.text, loadingMenu);
        }

        public void BTN_QuitGame()
        {
            //! spawn popup
        }

        public void BTN_CancelQuit()
        {

        }

        public void BTN_ActualQuit()
        {
            Application.Quit();
        }
        #endregion

        #region Room Phase
        public void StartRoomPhase()
        {
            OpenMenu(roomMenu);
            CloseMenu(loadingMenu);
            CloseMenu(lobbyMenu);

            findRoomFR.StartLerp(true);
            createRoomFR.StartLerp(true);
        }

        public void BTN_StartActualLevel()
        {
            SceneManager.LoadScene(nextLevelName);
        }

        public void BTN_LeaveRoom()
        {
            CloseMenu(roomMenu);
            OpenMenu(lobbyMenu);
            CloseMenu(roomOptions);
            OpenMenu(gameOptions);

            launcher.LeaveRoom();
        }
        #endregion

        #endregion
    }

}