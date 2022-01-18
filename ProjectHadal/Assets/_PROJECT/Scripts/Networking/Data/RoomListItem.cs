using System;
using UnityEngine;
using Photon.Realtime;
using TMPro;

namespace Hadal.Networking
{
    public class RoomListItem : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        public RoomInfo info;

        private bool hasSetup;
        private bool hasClicked = false;

        private void OnDestroy()
        {
            if (hasSetup)
            {
                NetworkEventManager.Instance.JoinRoomFailedEvent -= OnJoinRoomFailed;
            }
        }

        public void SetUp(RoomInfo _info)
        {
            info = _info;
            text.text = _info.Name;
            NetworkEventManager.Instance.JoinRoomFailedEvent += OnJoinRoomFailed;

            hasSetup = true;
        }

        public void OnClick()
        {
            if (hasClicked) return;
            hasClicked = true;
            NetworkEventManager.Instance.JoinRoom(info);
        }

        void OnJoinRoomFailed()
        {
            hasClicked = false;
        }
    }
}