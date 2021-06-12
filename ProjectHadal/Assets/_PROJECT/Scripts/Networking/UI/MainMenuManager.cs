using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Magthylius.LerpFunctions;
using Photon.Realtime;
using System.Collections.Generic;
using Hadal.Networking.UI.Loading;

//! C: Jon
namespace Hadal.Networking.UI.MainMenu
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
        LoadingManager loadingManager;

        bool mainMenuInitiated = false;

        [Header("Menu settings")]
        [SerializeField] Menu startMenu;
        [SerializeField] Menu nicknameMenu;
        [SerializeField] Menu lobbyMenu;
        [SerializeField] Menu connectingMenu;
        [SerializeField] Menu roomMenu;
        [SerializeField] GameObject connectingTMP;
        [SerializeField] GameObject loginTMP;
        [SerializeField] GameObject connectLMBPrompt;

        [Header("Title settings")]
        [SerializeField] MagthyliusPointerButton titleQuitButton;
        [SerializeField] TextMeshProUGUI versionTMP;

        //ImageFiller startIF;

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
        [SerializeField] Transform playerListContent;
        [SerializeField] GameObject playerListItemPrefab;
        [SerializeField] Transform roomListContent;
        [SerializeField] GameObject roomListItemPrefab;
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
        [SerializeField] TextMeshProUGUI roomNameText;
        public GameObject startGameButton;

        [Header("Quit Settings")]
        [SerializeField] RectTransform confirmQuitPanel;

        FlexibleRect confirmQuitFR;

        //! Connections
        bool onMaster = false;

        void Awake()
        {
            Instance = this;
            mainMenuInitiated = false;
        }

        void Start()
        {
            NetworkEventManager.Instance.JoinedLobbyEvent += EndStartPhase;
            
            DetermineMenuToOpen();
            //if (!NetworkEventManager.Instance.IsConnected) mainMenuInitiated = true;
            InitMainMenu();
            //p

        }

        void Update()
        {
            print(menuPhase);
            if (!mainMenuInitiated) return;

            switch (menuPhase)
            {
                case MenuPhase.START:
                    //startIF.Step(Time.unscaledDeltaTime);
                    if (Input.GetMouseButtonDown(0) && !titleQuitButton.IsHovered) ConnectToLobby();
                    //print("what the fuck");
                    break;
                case MenuPhase.MAIN:
                   // Debug.LogWarning("run");
                    if (createRoomFR != null) createRoomFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    if (findRoomFR != null) findRoomFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    if (confirmQuitFR != null) confirmQuitFR.Step(roomPanelLerpSpeed * Time.unscaledDeltaTime);
                    break;
            }
            
            //print("?");
        }

        void OnDestroy()
        {
            NetworkEventManager.Instance.JoinedLobbyEvent -= EndStartPhase;
        }

        #region Main Menu 
        public void InitMainMenu()
        {
            //Debug.LogWarning("init");
            loadingManager = LoadingManager.Instance;

            DetermineMenuToOpen();
            EnsureSetup(true);

            versionTMP.text = "V " + Application.version;

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

        //! make sure objects are active and inactive
        void EnsureSetup(bool openStartMenu = true)
        {
            if (openStartMenu)
                OpenMenu(startMenu);

            //print("?");
            OpenMenu(gameOptions);
            CloseMenu(nicknameMenu);
            CloseMenu(lobbyMenu);
            CloseMenu(roomOptions);
            CloseMenu(connectingMenu);

            connectLMBPrompt.SetActive(true);
            connectingTMP.SetActive(false);
            loginTMP.SetActive(true);

            createRoomPanel.gameObject.SetActive(true);
            findRoomPanel.gameObject.SetActive(true);
            confirmQuitPanel.gameObject.SetActive(true);

            warningNicknameTooLong.gameObject.SetActive(false);
            warningRoomNameTooLong.gameObject.SetActive(false);
        }

        void DetermineMenuToOpen()
        {
            print(NetworkEventManager.Instance.IsConnected);
            if (NetworkEventManager.Instance.IsConnected)
            {
                OpenMenu(lobbyMenu);
                menuPhase = MenuPhase.MAIN;
            }
            else
            {
                OpenMenu(startMenu);
                menuPhase = MenuPhase.START;
            }

            mainMenuInitiated = true;
            
        }

        /// <summary>
        /// Used for reseting main menu when in main menu only
        /// </summary>
        public void ResetMainMenu()
        {
            if (GameManager.Instance.IsInGame) return;

            //InitMainMenu();
            //EnsureSetup(false);
            OpenMenu(lobbyMenu);

            createRoomFR.MoveToEnd();
            findRoomFR.MoveToEnd();
            confirmQuitFR.MoveToEnd();
        }

        void ChangePhase(MenuPhase phase) => menuPhase = phase;

        #region Start phase
        //public void PNTR_ChargeStartFiller() => startIF.StartCharge();
        //public void PNTR_DischargeStartFiller() => startIF.StopCharge();
        void ConnectToLobby()
        {
            NetworkEventManager.Instance.ConnectUsingSettings();
            connectLMBPrompt.SetActive(false);
            connectingTMP.SetActive(true);

            //print("Connecting to lobby");
        }

        void EndStartPhase()
        {
            //InitMainMenu(); 
            //print("start");

            ChangePhase(MenuPhase.MAIN);
            CloseMenu(startMenu);
            //DetermineMenuToOpen();

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
            if (!allowNickname) return;

            PlayerPrefs.SetString("PlayerName", nicknameTMPInput.text);
            UpdateLobbyNickname();

            CloseMenu(nicknameMenu);
            OpenMenu(lobbyMenu);
        }

        void UpdateLobbyNickname()
        {
            lobbyNicknameTMP.text = PlayerPrefs.GetString("PlayerName").ToUpper();
            NetworkEventManager.Instance.ChangeNickname(PlayerPrefs.GetString("PlayerName"));
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
            if (!NetworkEventManager.Instance.InLobby) return;
            createRoomFR.StartLerp(false);
            findRoomFR.StartLerp(true); 
        }

        public void BTN_CreateActualRoom()
        {
            if (!allowRoomCreation) return;
            NetworkEventManager.Instance.CreateRoom(createRoomTMPInput.text);
            onMaster = false;
            connectingMenu.Open();
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

        public void BTN_StartActualLevel()
        {
            NetworkEventManager.Instance.SetCurrentRoomCustomProperty("s", NetworkEventManager.RoomState.STARTED);
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.GAME_START_LOAD, null);
            loadingManager.LoadLevel(NetworkEventManager.Instance.InGameScene);
        }

        public void BTN_LeaveRoom()
        {
            /*CloseMenu(roomMenu);
            OpenMenu(lobbyMenu);
            CloseMenu(roomOptions);
            OpenMenu(gameOptions);*/
            //InitMainMenu();
            //EnsureSetup(false);
            CloseMenu(roomMenu);
            CloseMenu(roomOptions);

            OpenMenu(lobbyMenu);
            OpenMenu(gameOptions);

            NetworkEventManager.Instance.LeaveRoom();
        }

        public void TMP_CheckRoomNameEligibility()
        {
            if (createRoomTMPInput.text.Length <= roomNameMaxLength) allowRoomCreation = true;
            else allowRoomCreation = false;

            warningRoomNameTooLong.SetActive(!allowRoomCreation);
        }
        #endregion

        #region Room Phase
        public void StartRoomPhase(string roomName)
        {
            roomNameText.text = roomName;

            OpenMenu(roomMenu);
            CloseMenu(connectingMenu);
            CloseMenu(lobbyMenu);

            findRoomFR.StartLerp(true);
            createRoomFR.StartLerp(true);
        }

        public void UpdatePlayerList(Player[] playerList)
        {
            foreach (Transform child in playerListContent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < playerList.Length; i++)
            {
                AddIntoPlayerList(playerList[i]);
            }
        }

        public void AddIntoPlayerList(Player player)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(player);
        }

        public void UpdateRoomList(List<RoomInfo> roomList)
        {
            foreach (Transform trans in roomListContent)
            {
                Destroy(trans.gameObject);
            }
            for (int i = 0; i < roomList.Count; i++)
            {
                //! If room have removed from the list
                if (roomList[i].RemovedFromList)
                    continue;
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
            }
        }


        #endregion

        #endregion

        public static bool IsNull => Instance == null;

        public bool OnMaster => onMaster;
        public void ConnectedToMaster() => onMaster = true;
    }

}