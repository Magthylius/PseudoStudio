using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
                DontDestroyOnLoad(gameObject);
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

        public void AddListener(ByteEvents eventCode, Action<EventData> action)
        {
            if (recieverDict.ContainsKey(eventCode))
            {
                if (recieverDict[eventCode] == null) recieverDict[eventCode] = action;
                else recieverDict[eventCode] += action;

                return;
            }
        }

        //! implement INetworkObject
    }
}
