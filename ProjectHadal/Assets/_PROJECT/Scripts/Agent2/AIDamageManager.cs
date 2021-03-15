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

namespace Hadal.AIComponents
{
    public class AIDamageManager : MonoBehaviour
    {
        public AIBrain Brain { get; private set; }
        [Header("Damage Values")]
        [Foldout("Damage Type"), SerializeField] int pinDamage;
        [Foldout("Damage Type"), SerializeField] int tailWhipDamage;
        List<PlayerController> players;

        private void Awake()
        {
            Brain = GetComponent<AIBrain>();
            Brain.GetViewIDMethod = (trans) => GetViewIDFromTransform(trans);
            Brain.ViewIDBelongsToTransMethod = (trans, id) => ViewIDBelongsToTransform(id, trans);
            // TODO: A better way to get playercontroller? maybe another script
            players = NetworkEventManager.Instance.PlayerObjects.Select(p => p.GetComponent<PlayerController>()).ToList();
            Brain.InjectPlayerTransforms(players.Select(p => p.transform).ToList());

            //! Subcribes damage player event
            AIBrain.DamagePlayerEvent += Send_DamagePlayer;
            NetworkEventManager.Instance.AddListener(NetworkEventManager.ByteEvents.AI_DAMAGE_EVENT, Receive_DamagePlayer);
        }

        private void OnDestroy()
        {
            //! Unsubscribe damage player event
            AIBrain.DamagePlayerEvent -= Send_DamagePlayer;
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
            NetworkEventManager.Instance.RaiseEvent(NetworkEventManager.ByteEvents.AI_DAMAGE_EVENT, data);
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
            
            players
                .Where(i => i.GetInfo.PhotonInfo.PView.ViewID == viewID)
                .First()
                .GetComponent<IDamageable>()
                .TakeDamage(damage);
        }

        public int GetViewIDFromTransform(Transform trans)
            => trans.GetComponentInChildren<PlayerController>().GetInfo.PhotonInfo.PView.ViewID;
        
        public bool ViewIDBelongsToTransform(int id, Transform trans)
            => GetViewIDFromTransform(trans) == id;
    }
}
