using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor.Graphs;

namespace Hadal.Networking
{
    public class PlayerListItem : MonoBehaviourPunCallbacks
    {
        [SerializeField] TMP_Text text;
        Player player;
        
        public void SetUp(Player _player, Color playerColor)
        {
            player = _player;
            text.text = _player.NickName;
            text.color = playerColor;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if(player == otherPlayer)
            {
                Destroy(gameObject);
            }
        }

        public override void OnLeftRoom()
        {
            Destroy(gameObject);
        }
    }
}