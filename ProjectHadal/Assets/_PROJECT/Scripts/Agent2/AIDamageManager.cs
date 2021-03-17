using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;
using Hadal.AI;
using Tenshi.UnitySoku;
using Tenshi;
using Photon.Pun;
using NaughtyAttributes;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using ReadOnly = Tenshi.ReadOnlyAttribute;

namespace Hadal.AIComponents
{
    public class AIDamageManager : MonoBehaviour
    {
        public AIBrain Brain { get; private set; }
        [Header("Damage Values")]
        [Foldout("Damage Type"), SerializeField] int pinDamage;
        [Foldout("Damage Type"), SerializeField] int tailWhipDamage;
        [ReadOnly, SerializeField] List<PlayerController> players;
        [ReadOnly, SerializeField] List<GameObject> playerObjects;

        private void Awake()
        {
            Brain = GetComponent<AIBrain>();
            Brain.GetViewIDMethod = (trans) => GetViewIDFromTransform(trans);
            Brain.ViewIDBelongsToTransMethod = (trans, id) => ViewIDBelongsToTransform(id, trans);
            
            //! Subcribes player events
            AIBrain.DamagePlayerEvent += Send_DamagePlayer;
            Brain.FreezePlayerMovementEvent += HandlePlayerMovementFreeze;
            Brain.ForceSlamPlayerEvent += HandlePlayerSlamEvent;
            NetworkEventManager.Instance.AddListener(ByteEvents.AI_DAMAGE_EVENT, Receive_DamagePlayer);
        }

        private void Start()
        {
            UpdatePlayerControllers();
        }

        private void OnDestroy()
        {
            //! Unsubscribe player events
            AIBrain.DamagePlayerEvent -= Send_DamagePlayer;
            Brain.FreezePlayerMovementEvent -= HandlePlayerMovementFreeze;
            Brain.ForceSlamPlayerEvent -= HandlePlayerSlamEvent;
        }

        private void Send_DamagePlayer(Transform player, AIDamageType type)
        {
            //! compute data
            int targetViewID = GetViewIDFromTransform(player);
            int damage = type switch
            {
                AIDamageType.Pin => pinDamage,
                AIDamageType.Tail => tailWhipDamage,
                _ => 0
            };

            //! raise event with data
            object[] data = { targetViewID, damage };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_DAMAGE_EVENT, data, options);
        }

        /// <summary> Damages the chosen player</summary>
        /// <param name="player">Target player</param>
        /// <param name="type">The damage type</param>
        private void Receive_DamagePlayer(EventData eventData)
        {
            object[] data = eventData.CustomData.AsObjArray();
            if (data == null) return;
            int viewID = data[0].AsInt();
            int damage = data[1].AsInt();

            if (players.IsNullOrEmpty())
                players = NetworkEventManager.Instance.PlayerObjects.Select(p => p.GetComponent<PlayerController>()).ToList();

            players
                .Where(i => i.GetInfo.PhotonInfo.PView.ViewID == viewID)
                .First()
                .GetComponentInChildren<IDamageable>()
                .TakeDamage(damage);
        }

        private void HandlePlayerMovementFreeze(Transform player, bool shouldFreeze)
        {
            var p = player.GetComponent<PlayerController>().Mover;
            if (shouldFreeze)
            {
                p.Disable();
            }
            else
            {
                p.Enable();
            }
        }

        private void HandlePlayerSlamEvent(Transform player, Vector3 destination)
        {
            var p = player.GetComponent<PlayerController>();
            var direction = (player.position - destination).normalized;
            p.AddVelocity(1000000f, direction);
        }

        private void UpdatePlayerControllers()
        {
            playerObjects = NetworkEventManager.Instance.PlayerObjects;
            players = NetworkEventManager.Instance.PlayerObjects.Select(p => p.GetComponent<PlayerController>()).ToList();
            Brain.InjectPlayerTransforms(players.Select(p => p.transform).ToList());
        }

        public int GetViewIDFromTransform(Transform trans)
            => trans.GetComponentInChildren<PlayerController>().GetInfo.PhotonInfo.PView.ViewID;

        public bool ViewIDBelongsToTransform(int id, Transform trans)
            => GetViewIDFromTransform(trans) == id;
    }
}
