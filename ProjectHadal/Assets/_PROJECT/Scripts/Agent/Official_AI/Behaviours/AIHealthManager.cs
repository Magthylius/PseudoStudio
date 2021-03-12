using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable
    {
        [SerializeField] int maxHealth;
        int currentHealth;

        private void Awake()
        {
            if (maxHealth <= 0) maxHealth = 1;
            ResetHealth();
        }

        public void CheckHealth()
        {
            if (IsUnalive)
            {
                $"U.N. Owen has been unalived".Msg();
                Obj.SetActive(false);
            }
        }

        public bool TakeDamage(int damage)
        {
            currentHealth = (currentHealth - damage).Clamp0();
            return true;
        }
        public GameObject Obj => gameObject;
        public bool IsUnalive => currentHealth <= 0;
        public float GetHealthRatio => currentHealth / maxHealth.AsFloat();

        private void ResetHealth() => currentHealth = maxHealth;
    }
}
