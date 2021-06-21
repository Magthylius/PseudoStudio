using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AIDamageHandler : MonoBehaviour, IDamageable
    {
        [SerializeField] private AIHealthManager healthManager;

        public bool TakeDamage(int damage)
        {
            return healthManager.TakeDamage(damage);
        }

        public GameObject Obj { get; }
    }
}
