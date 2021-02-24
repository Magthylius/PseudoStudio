using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

//! C: Jon
namespace Hadal
{
    public class NetworkEventManager : MonoBehaviourPunCallbacks
    {
        public static NetworkEventManager Instance;

        //public Action<EventData> EventReciever;

        public delegate void EventRecievedInvoker();
        public event EventRecievedInvoker EventReciever;

        List<EventRecievedInvoker> eventRecieverList;

        #region Byte Declarations
        public enum ByteEvents
        {
            PLAYER_UTILITIES_LAUNCH = 0,
            TOTAL_EVENTS
        }
        #endregion

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
                DontDestroyOnLoad(gameObject);
            }

        }

        void Start()
        {
            eventRecieverList = new List<EventRecievedInvoker>();
            for (int i = 0; i < (int)ByteEvents.TOTAL_EVENTS; i++)
            {
                //! init list
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            //PhotonNetwork.NetworkingClient.AddCallbackTarget(EventReciever);
            PhotonNetwork.NetworkingClient.EventReceived += InvokeRecievedEvents;
        }

        public void Event(ByteEvents eventCode, object dataContent)
        {
            PhotonNetwork.RaiseEvent((byte)eventCode, dataContent, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }

        public EventRecievedInvoker FindInvoker(ByteEvents eventCode)
        {
            //! return fire event
            return null;
        }

        void InvokeRecievedEvents(EventData obj)
        {

        }

        //! implement INetworkObject
    }
}
