using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using NaughtyAttributes;
using Hadal.Networking.UI.MainMenu;

//! C: Jon
namespace Hadal.Networking
{
    public enum ByteEvents
    {
        PLAYER_SPAWNED = 0,
        PLAYER_SPAWNED_CONFIRMED,
        GAME_START_LOAD,
        GAME_ACTUAL_START,
        PLAYER_UTILITIES_LAUNCH,
        PLAYER_TORPEDO_LAUNCH,
        PROJECTILE_DESPAWN,
        PROJECTILE_ACTIVATED,
        PROJECTILE_ATTACH,
        AI_SEND_DAMAGE,
        AI_RECEIVE_DAMAGE,
        AI_PIN_EVENT,
        AI_GRAB_PLAYER,
        AI_RELEASE_PLAYER,
        TOTAL_EVENTS
    }

    public delegate void ConnectionEvent();

    public class NetworkEventManager : MonoBehaviourPunCallbacks
    {
        public static NetworkEventManager Instance;

        [Header("Network Settings")]
        public bool isOfflineMode;
        public Transform localPlayerSpawnTrans;

        [Header("Scene References")]
        [Scene] public string MainMenuScene;
        [Scene] public string InGameScene;

        List<GameObject> playerObjects;

        [Header("Offline settings")] 
        [Range(0, 3)] public int DummyCount;
        public bool DummyMirrorsMovement;
        
        //! internal references
        bool loadsToMainMenu = false;

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null)
            {
                gameObject.name += " (Deprecated)";
                Destroy(this);
                return;
            }
            else
            {
                Instance = this;
            }

            SetupNetworking();
            //print("awake");

            playerObjects = new List<GameObject>();
            
            //! Force disconnect because PhotonNetwork does not disconnect after offline mode
            if (IsConnected && !isOfflineMode) Disconnect();
        }

        void Start()
        {
            SetupEssentials();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.NetworkingClient.EventReceived += InvokeRecievedEvents;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SetupEventRaising();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.NetworkingClient.EventReceived -= InvokeRecievedEvents;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Essentials
        GameManager gameManager;
        MainMenuManager mainMenuManager;

        void SetupEssentials()
        {
            gameManager = GameManager.Instance;
            mainMenuManager = MainMenuManager.Instance;
        }
        #endregion

        #region Raising Events
        Dictionary<ByteEvents, Action<EventData>> recieverDict;

        void SetupEventRaising()
        {
            recieverDict = new Dictionary<ByteEvents, Action<EventData>>();

            for (int i = 0; i < (int)ByteEvents.TOTAL_EVENTS; i++)
            {
                //! init dict
                recieverDict.Add((ByteEvents)i, null);
            }
        }

        /// <summary>Raise default event.</summary>
        /// <remarks>Be sure that event code needed is in the enumeration.</remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="dataContent">Custom data object to pass through events.</param>
        public void RaiseEvent(ByteEvents eventCode, object dataContent)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
        /// <summary>Raise event, with raise event options.</summary>
        /// <remarks>Be sure that event code needed is in the enumeration.</remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="dataContent">Custom data object to pass through events.</param>
        /// <param name="raiseEventOptions">Raise event options to define.</param>
        public void RaiseEvent(ByteEvents eventCode, object dataContent, RaiseEventOptions raiseEventOptions)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, raiseEventOptions, SendOptions.SendUnreliable);
        }
        /// <summary>Raise event, with send options.</summary>
        /// <remarks>Be sure that event code needed is in the enumeration.</remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="dataContent">Custom data object to pass through events.</param>
        /// <param name="sendOptions">Send options to define</param>
        public void RaiseEvent(ByteEvents eventCode, object dataContent, SendOptions sendOptions)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, RaiseEventOptions.Default, sendOptions);
        }
        /// <summary>Raise event, with raise event and send options.</summary>
        /// <remarks>Be sure that event code needed is in the enumeration.</remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="dataContent">Custom data object to pass through events.</param>
        /// <param name="raiseEventOptions">Raise event options to define.</param>
        /// <param name="sendOptions">Send options to define</param>
        public void RaiseEvent(ByteEvents eventCode, object dataContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, raiseEventOptions, sendOptions);
        }

        void InvokeRecievedEvents(EventData eventObject)
        {
            for (int i = 0; i < (int)ByteEvents.TOTAL_EVENTS; i++)
            {
                if (eventObject.Code == (byte)(ByteEvents)i)
                {
                    if (recieverDict.ContainsKey((ByteEvents)eventObject.Code))
                    {
                        if (recieverDict[(ByteEvents)eventObject.Code] != null) recieverDict[(ByteEvents)eventObject.Code].Invoke(eventObject);
                        return;
                    }
                }
            }
        }

        /// <summary>Attach invoked functions to listen to events as specified.</summary>
        /// <remarks>Be sure that event code needed is in the enumeration.</remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="action">Attached listener function.</param>
        public void AddListener(ByteEvents eventCode, Action<EventData> action)
        {
            if (recieverDict.ContainsKey(eventCode))
            {
                if (recieverDict[eventCode] == null) recieverDict[eventCode] = action;
                else recieverDict[eventCode] += action;

                return;
            }

            Debug.LogWarning(eventCode.ToString() + " is not found, listener unattached.");
        }

        /// <summary>Remove attached functions from listener.</summary>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="action">Target remove listener function.</param>
        public void RemoveListener(ByteEvents eventCode, Action<EventData> action)
        {
            if (recieverDict.ContainsKey(eventCode))
            {
                if (recieverDict[eventCode] != null)
                {
                    recieverDict[eventCode] -= action;
                    recieverDict.Remove(eventCode);
                    return;
                }
            }

            Debug.LogWarning(eventCode.ToString() + " is not found, unable to remove listener.");
        }
        #endregion

        #region Photon Networking Overrides
        public delegate void NetworkEventPlayer(Player player);
        public delegate void NetworkEvent();
        public event NetworkEventPlayer PlayerEnteredEvent;
        public event NetworkEventPlayer PlayerLeftEvent;
        public event NetworkEventPlayer MasterClientSwitchedEvent;
        public event NetworkEvent LeftRoomEvent;

        [Header("Room Options")]
        [Tooltip("Max number of players in room")]
        [SerializeField] int maxPlayers;
        [Tooltip("Time To Live of players, in milliseconds")]
        [SerializeField] int playerTTL;
        [Tooltip("Time To Live of room when last player leaves, in milliseconds")]
        [SerializeField] int emptyRoomTTL;
        [Tooltip("Cleans up props of a player when player leave")]
        [SerializeField] bool cleanupCache;

        //! Used to check if a room is started
        public enum RoomState
        {
            WAITING = 0,
            STARTED,
        }

        RoomOptions roomOptionsDefault;

        void SetupNetworking()
        {
            //if (!PhotonNetwork.IsConnected)
                //PhotonNetwork.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings, isOfflineMode);

            if (isOfflineMode)
                PhotonNetwork.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings, isOfflineMode);

            roomOptionsDefault = new RoomOptions();
            roomOptionsDefault.MaxPlayers = (byte)maxPlayers;
            roomOptionsDefault.PlayerTtl = playerTTL;
            roomOptionsDefault.EmptyRoomTtl = emptyRoomTTL;
            roomOptionsDefault.CleanupCacheOnLeave = cleanupCache;

            roomOptionsDefault.CustomRoomProperties = new Hashtable();
            roomOptionsDefault.CustomRoomProperties.Add("s", (int)RoomState.WAITING); //! Documentation says short key names is better
        }
        public void SetCurrentRoomCustomProperty(Hashtable hashTable)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashTable);
        }
        public void SetCurrentRoomCustomProperty(object key, object value)
        {
            print(PhotonNetwork.CurrentRoom);
            Hashtable hashTable = new Hashtable();
            hashTable.Add(key, value);
            SetCurrentRoomCustomProperty(hashTable);
        }

        [Button("Force disconnect")]
        public void Disconnect() => PhotonNetwork.Disconnect();
        public void ChangeNickname(string nickname) => PhotonNetwork.NickName = nickname;
        public void CreateRoom(string roomName) => PhotonNetwork.CreateRoom(roomName, roomOptionsDefault);
        public void JoinRoom(RoomInfo roomInfo) => PhotonNetwork.JoinRoom(roomInfo.Name);
        public void LeaveRoom(bool returnsToMainMenu = false)
        {
            PhotonNetwork.LeaveRoom();

            if (returnsToMainMenu)
            {
                if (LeftRoomEvent != null) LeftRoomEvent.Invoke();
                //LoadLevel(MainMenuScene);
                loadsToMainMenu = true;
            }
        }
        public void LoadLevel(int index) => PhotonNetwork.LoadLevel(index);
        public void LoadLevel(string levelName) => PhotonNetwork.LoadLevel(levelName);
        public AsyncOperation LoadLevelAsync(int index)
        {
            PhotonNetwork.LoadLevel(index);
            return PhotonNetwork._AsyncLevelLoadingOperation;
        }
        public AsyncOperation LoadLevelAsync(string levelName)
        {
            PhotonNetwork.LoadLevel(levelName);
            return PhotonNetwork._AsyncLevelLoadingOperation;
        }

        #region Connection Functions
        public void ConnectUsingSettings()
        {
            if (PeerState != PeerStateValue.Disconnected) return;
            PhotonNetwork.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings, isOfflineMode);
        }

        public override void OnConnectedToMaster()
        {
            //Debug.Log("Connected to Master");
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;

            if (!MainMenuManager.IsNull) MainMenuManager.Instance.ConnectedToMaster();
        }

        public override void OnConnected()
        {
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected from server for reason " + cause.ToString());
            //print(MainMenuScene);
            LoadLevel(MainMenuScene);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (MasterClientSwitchedEvent != null) MasterClientSwitchedEvent.Invoke(newMasterClient);
        }
        #endregion

        #region Connection Events
        public event ConnectionEvent JoinedLobbyEvent;
        #endregion

        #region Room Functions
        public override void OnCreatedRoom()
        {
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public override void OnJoinedRoom()
        {
            object state;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("s", out state);
            RoomState roomState = (RoomState)state;

            if (roomState == RoomState.WAITING)
            {
                mainMenuManager.StartRoomPhase(PhotonNetwork.CurrentRoom.Name);

                Player[] players = PhotonNetwork.PlayerList;
                mainMenuManager.UpdatePlayerList(players);

                mainMenuManager.startGameButton.SetActive(PhotonNetwork.IsMasterClient);
            }  
            else
            {
                gameManager.ChangeGameState(GameManager.GameState.ONGOING);
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            mainMenuManager.AddIntoPlayerList(newPlayer);
            if (PlayerLeftEvent != null) PlayerEnteredEvent.Invoke(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (PlayerLeftEvent != null) PlayerLeftEvent.Invoke(otherPlayer);
        }

        public override void OnLeftRoom()
        {
            //! If not in mainmenu, return to mainmenu
            if (loadsToMainMenu)
            {
                LoadLevel(MainMenuScene);
                loadsToMainMenu = false;
            }
            else if (!gameManager.IsInGame)
            {
                mainMenuManager.ResetMainMenu();
            }
        }
        #endregion

        #region Lobby Functions
        public override void OnJoinedLobby()
        {           
            mainMenuManager = MainMenuManager.Instance;
            // mainMenuManager.InitMainMenu();
            print("Joined lobby");
            JoinedLobbyEvent?.Invoke();
            gameManager.ChangeGameState(GameManager.GameState.IDLE);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public override void OnLeftLobby()
        {
            print("Left lobby");
        }

        public override void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            if (!gameManager.IsInMainMenu) return;
            mainMenuManager.UpdateRoomList(roomList);
        }
        #endregion

        #region Data Functions
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        public override void OnWebRpcResponse(OperationResponse response)
        {
        }

        public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }

        public override void OnErrorInfo(ErrorInfo errorInfo)
        {
        }
        #endregion
        #endregion

        #region Room Management
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //print("scene");

            if (isOfflineMode) return;

            if (scene.name == InGameScene)
            {
                //! Create player manager
                if (PhotonNetwork.IsMasterClient)
                {
                    SpawnPlayerManager();
                    //print(PhotonNetwork.IsMasterClient);
                }

                GameManager.Instance.ChangeGameState(GameManager.GameState.ONGOING);
                //SpawnAIEssentials();
            }
        }

        public void AddPlayer(GameObject playerObject)
        {
            //print("player added");
            foreach (GameObject player in playerObjects) if (player == playerObject) return;
            playerObjects.Add(playerObject);
        }
        public void RemovePlayer(GameObject playerObject)
        {
            foreach (GameObject player in playerObjects)
            {
                if (player == playerObject)
                {
                    playerObjects.Remove(playerObject);
                    return;
                }
            }
        }
        public void SyncPlayerObjects(List<GameObject> playerObjectLists)
        {
            playerObjects = new List<GameObject>(playerObjectLists);
        }

        public void SpawnPlayerManager()
        {
            GameObject o = PhotonNetwork.Instantiate(PathManager.PlayerManagerPrefabPath, Vector3.zero, Quaternion.identity);
        }

        public void SpawnAIEssentials(Vector3 pos = new Vector3(), Quaternion rot = new Quaternion())
        {
            PhotonNetwork.Instantiate(PathManager.AIEssentialsPrefabPath, pos, rot);
        }

        public void SpawnAI(Vector3 pos = new Vector3(), Quaternion rot = new Quaternion())
        {
            PhotonNetwork.Instantiate(PathManager.AIPrefabPath, pos, rot);
        }
        #endregion

        #region Accessors
        
        public Transform LocalSpawn => localPlayerSpawnTrans;
        public Room CurrentRoom => PhotonNetwork.CurrentRoom;
        public Player LocalPlayer => PhotonNetwork.LocalPlayer;
        public Dictionary<int, Player> AllPlayers => PhotonNetwork.CurrentRoom.Players;
        public List<GameObject> PlayerObjects => playerObjects;
        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool InLobby => PhotonNetwork.InLobby;
        public bool InRoom => PhotonNetwork.InRoom;
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public PeerStateValue PeerState => PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState;
        #endregion
    }
}
