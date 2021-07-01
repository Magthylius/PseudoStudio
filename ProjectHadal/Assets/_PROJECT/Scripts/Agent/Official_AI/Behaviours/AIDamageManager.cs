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
        public int ThreshDamage { get { return threshDamage; } set { threshDamage = value; } }
        //! How long to damage
        [SerializeField] int threshTimer;
        public int ThreshTimer { get { return threshTimer; } set { threshTimer = value; } }
        //! How frequent to damage
        [SerializeField] float applyEveryNSeconds;
        public float ApplyEveryNSeconds { get { return applyEveryNSeconds; } set { applyEveryNSeconds = value; } }

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

        /// <summary> Damages the chosen player over the network.</summary>
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
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.SEND_PLAYER_DAMAGE, data, options, SendOptions.SendReliable);
        }

        public int GetViewIDFromTransform(Transform trans)
            => trans.GetComponentInChildren<PlayerController>().GetInfo.PhotonInfo.PView.ViewID;
    }
}
