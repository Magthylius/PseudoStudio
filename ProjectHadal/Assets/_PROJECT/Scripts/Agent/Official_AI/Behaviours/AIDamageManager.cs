using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;
using Tenshi.UnitySoku;
using Tenshi;
using Photon.Pun;
using NaughtyAttributes;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Hadal.AI
{
    public class AIDamageManager : MonoBehaviour, ILeviathanComponent
    {
        public AIBrain Brain { get; private set; }

        [Header("Damage Values")]
        [Foldout("Damage Type"), SerializeField] int threshDamage;
        [Foldout("Damage Type"), SerializeField] int tailWhipDamage;

        public void Initialise(AIBrain brain)
        {
            Brain = brain;
        }

        public void DoUpdate(in float deltaTime)
        {
        }

        public void DoFixedUpdate(in float fixedDeltaTime)
        {
        }

        public void DoLateUpdate(in float deltaTime)
        {
        }

        public UpdateMode LeviathanUpdateMode => UpdateMode.MainUpdate;

        /// <summary> Damages the chosen player</summary>
        /// <param name="player">Target player</param>
        /// <param name="type">The damage type</param>
        public void Send_DamagePlayer(Transform player, AIDamageType type)
        {
            //! compute data
            int targetViewID = GetViewIDFromTransform(player);
            int damage = type switch
            {
                AIDamageType.Thresh => threshDamage,
                AIDamageType.Tail => tailWhipDamage,
                _ => 0
            };

            //! raise event with data
            object[] data = { targetViewID, damage };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_DAMAGE_EVENT, data, options);
        }

        private void Receive_DamagePlayer(EventData eventData)
        {
            object[] data = eventData.CustomData.AsObjArray();
            if (data == null) return;
            int viewID = data[0].AsInt();
            int damage = data[1].AsInt();

            PlayerController player = Brain.Players.Where(p => p.GetInfo.PhotonInfo.PView.ViewID == viewID).FirstOrDefault();
            if (player == null)
            {
                Brain.RefreshPlayerReferences();
                player = Brain.Players.Where(p => p.GetInfo.PhotonInfo.PView.ViewID == viewID).FirstOrDefault();
            }

            if (player == null) { $"Cannot find player with view ID of {viewID}!".Msg(); return; }
            player.GetComponentInChildren<IDamageable>().TakeDamage(damage);
        }

        private void HandlePlayerThreshEvent(Transform player, Vector3 destination)
        {
            var p = player.GetComponent<PlayerController>();
            //! DoT?
        }

        private void UpdatePlayerControllers()
        {
            // playerObjects = NetworkEventManager.Instance.PlayerObjects;
            // players = NetworkEventManager.Instance.PlayerObjects.Select(p => p.GetComponent<PlayerController>()).ToList();
            // Brain.InjectPlayerTransforms(players.Select(p => p.transform).ToList());
        }

        public int GetViewIDFromTransform(Transform trans)
            => trans.GetComponentInChildren<PlayerController>().GetInfo.PhotonInfo.PView.ViewID;

        public bool ViewIDBelongsToTransform(int id, Transform trans)
            => GetViewIDFromTransform(trans) == id;


    }
}
