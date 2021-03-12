using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Hadal.Networking.UI.MainMenu;

//! C: Jon
namespace Hadal.Networking
{
    public class NetworkEventManager : MonoBehaviourPunCallbacks
    {
        public static NetworkEventManager Instance;

        [Header("Network Settings")]
        public bool isOfflineMode;

        [Header("Scene References")]
        [Scene] public string MainMenuScene;
        [Scene] public string InGameScene;

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
        }

        void Start()
        {
            SetupEssentials();
            SetupEventRaising();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.NetworkingClient.EventReceived += InvokeRecievedEvents;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.NetworkingClient.EventReceived -= InvokeRecievedEvents;
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
        public enum ByteEvents
        {
            PLAYER_UTILITIES_LAUNCH = 0,
            TOTAL_EVENTS
        }

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

        /// <summary>
        /// Raise default event.
        /// </summary>
        /// <remarks>
        /// Be sure that event code needed is in the enumeration.
        /// </remarks>
        /// <param name="eventCode">Event code defined in enum to call events.</param>
        /// <param name="dataContent">Custom data object to pass through events.</param>
        public void RaiseEvent(ByteEvents eventCode, object dataContent)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }

        void InvokeRecievedEvents(EventData eventObject)
        {
            for (int i = 0; i < (int)ByteEvents.TOTAL_EVENTS; i++)
            {
                if (eventObject.Code == (byte)(ByteEvents)i)
                {
                    if (recieverDict.ContainsKey((ByteEvents)eventObject.Code))
                    {
                        recieverDict[(ByteEvents)eventObject.Code].Invoke(eventObject);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Attach invoked functions to listen to events as specified.
        /// </summary>
        /// <remarks>
        /// Be sure that event code needed is in the enumeration.
        /// </remarks>
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
        #endregion

        #region Photon Networking Overrides
        public delegate void NetworkEvent(Player player);
        public event NetworkEvent PlayerEnteredEvent;
        public event NetworkEvent PlayerLeftEvent;
        public event NetworkEvent MasterClientSwitchedEvent;

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
            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings, isOfflineMode);

            roomOptionsDefault = new RoomOptions();
            roomOptionsDefault.MaxPlayers = (byte)maxPlayers;
            roomOptionsDefault.PlayerTtl = playerTTL;
            roomOptionsDefault.EmptyRoomTtl = emptyRoomTTL;
            roomOptionsDefault.CleanupCacheOnLeave = cleanupCache;

            roomOptionsDefault.CustomRoomProperties = new Hashtable();
            roomOptionsDefault.CustomRoomProperties.Add("s", RoomState.WAITING); //! Documentation says short key names is better
        }

        public void SetCurrentRoomCustomProperty(Hashtable hashTable)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashTable);
        }
        public void SetCurrentRoomCustomProperty(object key, object value)
        {
            Hashtable hashTable = new Hashtable();
            hashTable.Add(key, value);
            SetCurrentRoomCustomProperty(hashTable);
        }

        public void Disconnect() => PhotonNetwork.Disconnect();
        public void ChangeNickname(string nickname) => PhotonNetwork.NickName = nickname;
        public void CreateRoom(string roomName) => PhotonNetwork.CreateRoom(roomName, roomOptionsDefault);
        public void JoinRoom(RoomInfo roomInfo) => PhotonNetwork.JoinRoom(roomInfo.Name);
        public void LeaveRoom(bool returnsToMainMenu = false)
        {
            PhotonNetwork.LeaveRoom();

            if (returnsToMainMenu)
            {
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
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;
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
            MasterClientSwitchedEvent.Invoke(newMasterClient);
        } 
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

            print("enumeration");
            foreach (object key in PhotonNetwork.CurrentRoom.CustomProperties.Values)
            {
                print(key);
            }

            print(state);
            print((RoomState)state);
            RoomState roomState = (RoomState)state;

            if (roomState == RoomState.WAITING)
            {
                mainMenuManager.StartRoomPhase(PhotonNetwork.CurrentRoom.Name);

                Player[] players = PhotonNetwork.PlayerList;
                mainMenuManager.UpdatePlayerList(players);

                mainMenuManager.startGameButton.SetActive(PhotonNetwork.IsMasterClient);
            }  
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            mainMenuManager.AddIntoPlayerList(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public override void OnLeftRoom()
        {
            //! If not in mainmenu, return to mainmenu
            if (loadsToMainMenu)
            {
                LoadLevel(MainMenuScene);
                loadsToMainMenu = false;
            }
        }
        #endregion

        #region Lobby Functions
        public override void OnJoinedLobby()
        {
            Debug.Log("Joined Lobby");
            //PhotonNetwork.NickName = "Player " + Random.Range(0, 10).ToString("00");
            /*if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
                SetupNetworking();*/

            SetupNetworking();
            mainMenuManager = MainMenuManager.Instance;
            mainMenuManager.InitMainMenu();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public override void OnLeftLobby()
        {
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
    }
}
