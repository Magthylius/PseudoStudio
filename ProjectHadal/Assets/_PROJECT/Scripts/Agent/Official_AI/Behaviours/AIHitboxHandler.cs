using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;
using Tenshi;

namespace Hadal.AI
{
    public class AIHitboxHandler : MonoBehaviour, IDamageable, IStunnable, ISlowable
    {
		[SerializeField] private float hitboxDamageMultiplier = 1f;
        [SerializeField, ReadOnly] private AIHealthManager healthManager;
		
		public void Initialise(AIHealthManager healthManager, int layer)
		{
			this.healthManager = healthManager;
			gameObject.layer = layer;
		}

        public bool TakeDamage(int damage)
        {
			int totalDamage = (damage * hitboxDamageMultiplier).Round();
			
            //! if host
            if (NetworkEventManager.Instance.IsMasterClient)
                return healthManager.TakeDamage(totalDamage);
            //! if not host
            else
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_DAMAGE, totalDamage, SendOptions.SendReliable);
                return true;
            }
        }
        
        public bool TryStun(float duration) => healthManager.TryStun(duration);
        public void AttachProjectile() => healthManager.AttachProjectile();
        public void DetachProjectile() => healthManager.DetachProjectile();

        public GameObject Obj => gameObject;
    }
}
