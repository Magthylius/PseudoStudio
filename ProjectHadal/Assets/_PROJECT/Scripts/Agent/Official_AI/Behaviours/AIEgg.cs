using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.States;
using Tenshi;

namespace Hadal.AI
{
    public class AIEgg : MonoBehaviour, IDamageable
    {
        [SerializeField] int maxHealth = 40;
        int curHealth;

        public GameObject Obj => throw new System.NotImplementedException();

        public delegate void MaxConfidenceOnEggDestroyed(bool isEggDestroyed);
        public event MaxConfidenceOnEggDestroyed eggDestroyedEvent;

        void Start()
        {
            curHealth = maxHealth;
        }

        void Update()
        {
            CheckEggDestroyed();
        }

        void CheckEggDestroyed()
        {
            if(curHealth <= 0)
            {
                eggDestroyedEvent?.Invoke(true);
            }
        }

        public bool TakeDamage(int damage)
        {
            if (curHealth <= 0) return false;
            curHealth -= damage.Abs();
            return true;
        }
    }
}
