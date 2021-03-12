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

namespace Hadal.AIComponents
{
    public class AIDamageManager : MonoBehaviour
    {
        public AIBrain Brain { get; private set; }
        public List<Transform> playerTransforms;

        private void Awake()
        {
            Brain = GetComponent<AIBrain>();

            //! Subcribes damage player event
            AIBrain.DamagePlayerEvent += DamagePlayer;
        }

        private void Update()
        {
            if (playerTransforms.IsNullOrEmpty())
            {
                Brain.InjectPlayerTransforms(GetPlayers());
            }
            
        }
        
        private void OnDestroy()
        {
            //! Unsubscribe damage player event
            AIBrain.DamagePlayerEvent -= DamagePlayer;
        }

        /// <summary> Damages the chosen player</summary>
        /// <param name="player">Target player</param>
        /// <param name="damage">The damage to deal</param>
        private void DamagePlayer(Transform player, int damage)
        {
            if (player == null) return;
            IDamageable pDamagable = player.GetComponent<IDamageable>();
            if (pDamagable == null) return;
            pDamagable.TakeDamage(damage);
        }

        [Button("GetPlayers")]
        /// <summary>Find players using their transform</summary>
        List<Transform> GetPlayers()
        {
            var pT = new List<Transform>();
            pT = FindObjectsOfType<PlayerController>()
                           .Select(o => o.transform)
                           .ToList();
            $"Found {pT.Count} players".Msg();

            playerTransforms = new List<Transform>(pT);
            return pT;
        }
    }
}
