using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

//! E: Jon
namespace Hadal.Legacy
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public static Launcher Instance;

        [Header("Room Settings")]
        //[SerializeField] Menu roomMenu;
        //[SerializeField] Menu lobbyMenu;

        [Header("Text Settings")]
        [SerializeField] TMP_InputField roomNameInputField;
        [SerializeField] TMP_Text errorText;
        [SerializeField] TMP_Text roomNameText;

        [Header("Spawn Settings")]
        [SerializeField] Transform roomListContent;
        [SerializeField] Transform playerListContent;
        [SerializeField] GameObject roomListItemPrefab;
        [SerializeField] GameObject playerListItemPrefab;

        [Header("Scene Settings")]
        [SerializeField] GameObject startGameNicoButton;
        [SerializeField] GameObject startGameJeyButton;

        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            Debug.Log("Connecting to Master");
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            PhotonNetwork.JoinLobby();
            //! Photon network auto determines cilent's scene to sync based on switch of scenes on host
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public override void OnJoinedLobby()
        {
            //MainMenuManager.Instance.OpenMenu("title");
            Debug.Log("Joined Lobby");
            PhotonNetwork.NickName = "Player " + Random.Range(0, 10).ToString("00");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected from server for reason " + cause.ToString());
        }

        public void CreateRoom()
        {
            if (string.IsNullOrEmpty(roomNameInputField.text))
            {
                return;
            }
            PhotonNetwork.CreateRoom(roomNameInputField.text);
            //MainMenuManager.Instance.OpenMenu("loading");
        }

        /*public void CreateRoom(string roomName, Menu loadMenu)
        {
            if (string.IsNullOrEmpty(roomName)) return;

            PhotonNetwork.CreateRoom(roomName);
            //MainMenuManager.Instance.OpenMenu(loadMenu);
        }*/

        public override void OnJoinedRoom()
        {
            //MainMenuManager.Instance.StartRoomPhase();
            roomNameText.text = PhotonNetwork.CurrentRoom.Name;


            Player[] players = PhotonNetwork.PlayerList;

            //!Clear player in playerListContent
            foreach (Transform child in playerListContent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < players.Length; i++)
            {
                //Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
            }

            //! If host, button is set active
            //startGameNicoButton.SetActive(PhotonNetwork.IsMasterClient);
            //startGameJeyButton.SetActive(PhotonNetwork.IsMasterClient);

            //MainMenuManager.Instance.startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        }

        //! Photon has in built where host leaves, a new host will be chosen, during switch, active the game start button
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            startGameNicoButton.SetActive(PhotonNetwork.IsMasterClient);
            startGameJeyButton.SetActive(PhotonNetwork.IsMasterClient);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            errorText.text = "Room Creation Failed" + message;
            //MainMenuManager.Instance.OpenMenu("error");
        }

        /// <summary>
        /// Start the game
        /// </summary>
        public void StartGame(int index)
        {
            PhotonNetwork.LoadLevel(index);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            //MainMenuManager.Instance.OpenMenu("loading");
        }

        public void JoinRoom(RoomInfo info)
        {
            PhotonNetwork.JoinRoom(info.Name);
            //MainMenuManager.Instance.OpenMenu("loading");
        }

        public override void OnLeftRoom()
        {
            //MainMenuManager.Instance.OpenMenu("title");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            /*foreach (Transform trans in roomListContent)
            {
                Destroy(trans.gameObject);
            }
            for (int i = 0; i < roomList.Count; i++)
            {
                //! If room have removed from the list
                if (roomList[i].RemovedFromList)
                    continue;
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
            }*/
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
        }

        public void ChangeNickname(string name)
        {
            PhotonNetwork.NickName = name;
        }
    }
}