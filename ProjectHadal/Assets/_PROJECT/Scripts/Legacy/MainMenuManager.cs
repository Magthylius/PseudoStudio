using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Magthylius.LerpFunctions;
using UnityEngine.SceneManagement;
using Photon.Pun;

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
        [SerializeField] Menu connectingMenu;
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
        [Min(0)] public int nicknameMaxLength;
        [SerializeField] GameObject warningNicknameTooLong;

        bool allowNickname;

        [Header("Lobby settings")]
        [SerializeField] Menu gameOptions;
        [SerializeField] Menu roomOptions;

        [Header("Room Joining settings")]
        [Min(0f)] public float roomPanelLerpSpeed;
        [SerializeField] RectTransform createRoomPanel;
        [SerializeField] RectTransform findRoomPanel;
       
        FlexibleRect createRoomFR;
        FlexibleRect findRoomFR;

        [Header("Room Creation settings")]
        [Min(0)] public int roomNameMaxLength;
        [SerializeField] TMP_InputField createRoomTMPInput;
        [SerializeField] GameObject warningRoomNameTooLong;

        bool allowRoomCreation;

        [Header("Room Ready settings")]
        public GameObject startGameButton;
        [SerializeField] string nextLevelName;

        [Header("Quit Settings")]
        [SerializeField] RectTransform confirmQuitPanel;

        FlexibleRect confirmQuitFR;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            EnsureSetup();

            versionTMP.text = "V " + Application.version;

            startIF = new ImageFiller(startFiller, startFillerSpeed, 1f);
            startIF.OnFillComplete += EndStartPhase;

            createRoomFR = new FlexibleRect(createRoomPanel);
            createRoomFR.SetTargetPosition(createRoomFR.GetBodyOffset(Vector2.right));
            createRoomFR.MoveToEnd();

            findRoomFR = new FlexibleRect(findRoomPanel);
            findRoomFR.SetTargetPosition(findRoomFR.GetBodyOffset(Vector2.right));
            findRoomFR.MoveToEnd();

            confirmQuitFR = new FlexibleRect(confirmQuitPanel);
            confirmQuitFR.SetTargetPosition(confirmQuitFR.GetBodyOffset(Vector2.right));
            confirmQuitFR.MoveToEnd();
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
                    confirmQuitFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    break;
            }
        }

        #region Main Menu 
        void ChangePhase(MenuPhase phase) => menuPhase = phase;

        //! make sure objects are active and inactive
        void EnsureSetup()
        {
            OpenMenu(startMenu);
            OpenMenu(gameOptions);
            CloseMenu(nicknameMenu);
            CloseMenu(lobbyMenu);
            CloseMenu(roomOptions);
            CloseMenu(connectingMenu);

            createRoomPanel.gameObject.SetActive(true);
            findRoomPanel.gameObject.SetActive(true);
            confirmQuitPanel.gameObject.SetActive(true);

            warningNicknameTooLong.gameObject.SetActive(false);
            warningRoomNameTooLong.gameObject.SetActive(false);
        }

        #region Start phase
        public void PNTR_ChargeStartFiller() => startIF.StartCharge();
        public void PNTR_DischargeStartFiller() => startIF.StopCharge();
        void EndStartPhase()
        {
            ChangePhase(MenuPhase.MAIN);
            CloseMenu(startMenu);

            if (PlayerPrefs.HasKey("PlayerName") || true)
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
            if (!allowNickname) return;

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

        public void TMP_CheckNicknameEligibility()
        {
            if (nicknameTMPInput.text.Length <= nicknameMaxLength) allowNickname = true;
            else allowNickname = false;

            warningNicknameTooLong.SetActive(!allowNickname);
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
            if (!allowRoomCreation) return;
            launcher.CreateRoom(createRoomTMPInput.text, connectingMenu);
        }

        public void BTN_QuitGame()
        {
            confirmQuitFR.StartLerp(false);
        }

        public void BTN_CancelQuit()
        {
            confirmQuitFR.StartLerp(true);
        }

        public void BTN_ActualQuit()
        {
            Application.Quit();
        }

        public void TMP_CheckRoomNameEligibility()
        {
            if (createRoomTMPInput.text.Length <= roomNameMaxLength) allowRoomCreation = true;
            else allowRoomCreation = false;

            warningRoomNameTooLong.SetActive(!allowRoomCreation);
        }
        #endregion

        #region Room Phase
        public void StartRoomPhase()
        {
            OpenMenu(roomMenu);
            CloseMenu(connectingMenu);
            CloseMenu(lobbyMenu);

            findRoomFR.StartLerp(true);
            createRoomFR.StartLerp(true);
        }

        public void BTN_StartActualLevel()
        {
            if(PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(nextLevelName);
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