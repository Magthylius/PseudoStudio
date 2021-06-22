using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;

namespace Hadal.AI
{
    public class AIHitboxHandler : MonoBehaviour, IDamageable, IStunnable
    {
        [SerializeField] private AIHealthManager healthManager;

        public bool TakeDamage(int damage)
        {
            //! if host
            if (NetworkEventManager.Instance.IsMasterClient)
                return healthManager.TakeDamage(damage);
            //! if not host
            else
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_DAMAGE, damage, SendOptions.SendReliable);
                return true;
            }
        }
        
        public bool TryStun(float duration)
        {
            if (NetworkEventManager.Instance.IsMasterClient)
                return healthManager.TryStun(duration);
            else
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_STUN, null, SendOptions.SendReliable);
                return true;
            }
        }

        public GameObject Obj { get; }
        
    }
}
