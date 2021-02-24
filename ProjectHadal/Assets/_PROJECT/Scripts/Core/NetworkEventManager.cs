using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;

//! C: Jon
namespace Hadal
{
    public class NetworkEventManager : MonoBehaviourPunCallbacks
    {
        public static NetworkEventManager Instance;

        public enum ByteEvents
        {
            PLAYER_UTILITIES_LAUNCH = 0,
            TOTAL_EVENTS
        }

        Dictionary<ByteEvents, Action<EventData>> recieverDict;

        void Awake()
        {
            if (Instance != null)
            {
                gameObject.name += " (Deprecated)";
                Destroy(this);
            }
            else
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            recieverDict = new Dictionary<ByteEvents, Action<EventData>>();

            for (int i = 0; i < (int)ByteEvents.TOTAL_EVENTS; i++)
            {
                //! init dict
                recieverDict.Add((ByteEvents)i, null);
            }
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

        #region Raising Events
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

        #region Room Events
        void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnConnected()
        {
        }

        public override void OnLeftRoom()
        {
            //! If not in mainmenu, return to mainmenu
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public override void OnCreatedRoom()
        {
        }

        public override void OnJoinedLobby()
        {
        }

        public override void OnLeftLobby()
        {
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
        }

        public override void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }

        public override void OnJoinedRoom()
        {
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public override void OnConnectedToMaster()
        {
        }

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
    }
}
