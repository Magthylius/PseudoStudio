using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.States;
using Tenshi;

namespace Hadal.AI
{
    public class AIEgg : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] int maxHealth = 40;
        [SerializeField, ReadOnly] int curHealth;

        [Header("VFX")]
        [SerializeField, ReadOnly] private Transform[] randomHitPoints;
        [SerializeField] private VFXData vfx_OnDamaged;
        [SerializeField] private int vfxCountPerHit = 2;
        [SerializeField] private int vfxCountPerDeath = 8;

        public GameObject Obj => gameObject;

        public delegate void MaxConfidenceOnEggDestroyed(bool isEggDestroyed);
        public event MaxConfidenceOnEggDestroyed eggDestroyedEvent;

        void Awake()
        {
            curHealth = maxHealth;
        }

        void CheckEggDestroyed()
        {
            if (curHealth <= 0)
            {
                if (randomHitPoints.IsNotEmpty())
                {
                    int i = -1;
                    while (++i < vfxCountPerDeath)
                        PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
                }
                eggDestroyedEvent?.Invoke(true);
            }
        }

        void DoOnHitEffects(int damage)
        {
            if (randomHitPoints.IsNotEmpty() && curHealth > 0)
            {
                int i = -1;
                while (++i < vfxCountPerHit)
                    PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
            }
        }

        public bool TakeDamage(int damage)
        {
            if (curHealth <= 0) return false;
            damage = damage.Abs();
            curHealth -= damage;
            CheckEggDestroyed();
            DoOnHitEffects(damage);
            return true;
        }

        private void PlayVFXAt(VFXData vfx, Vector3 position)
        {
            if (vfx == null) return;
            vfx.SpawnAt(position);
        }

        private Vector3 GetRandomHitPosition() => randomHitPoints.RandomElement().position;
    }
}
