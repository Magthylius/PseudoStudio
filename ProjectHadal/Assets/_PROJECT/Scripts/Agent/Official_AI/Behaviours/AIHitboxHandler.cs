using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;

namespace Hadal.AI
{
    public class AIHitboxHandler : MonoBehaviour, IDamageable, IStunnable, ISlowable
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
            //Debug.LogWarning("I kena fucking stun");
            if (NetworkEventManager.Instance.IsMasterClient)
                return healthManager.TryStun(duration);
            else
            {
                //Debug.LogWarning("Stun event sent");
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_STUN, duration, SendOptions.SendReliable);
                return true;
            }
        }
        
        public void AttachProjectile()
        {
			healthManager.AttachProjectile();
            
			/*
			//Debug.LogWarning("Slower attached!");
            if (NetworkEventManager.Instance.IsMasterClient)
                healthManager.UpdateSlowStacks(1);
            else
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, 1, SendOptions.SendReliable);
			*/
        }

        public void DetachProjectile()
        {
			healthManager.DetachProjectile();
			/*
            //Debug.LogWarning("Slower detached!");
            if (NetworkEventManager.Instance.IsMasterClient)
                healthManager.UpdateSlowStacks(-1);
            else
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, -1, SendOptions.SendReliable);
			*/
        }

        public GameObject Obj { get; }
    }
}
