using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, ILeviathanComponent
    {
        [SerializeField] int maxHealth;
        int currentHealth;
        AIBrain brain;

        public void Initialise(AIBrain brain)
        {
            this.brain = brain;
            if (maxHealth <= 0) maxHealth = 1;
            ResetHealth();
        }
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }
        public void DoLateUpdate(in float deltaTime)
        {
            CheckHealthStatus();
        }

        public void CheckHealthStatus()
        {
            if (IsUnalive)
            {
                $"Leviathan is unalive. Congrats!!!".Msg();
                Obj.SetActive(false);
            }
        }

        public bool TakeDamage(int damage)
        {
            currentHealth = (currentHealth - damage).Clamp0();
            $"AI health: {currentHealth}".Msg();
            return true;
        }
        public GameObject Obj => transform.parent.gameObject;
        public bool IsUnalive => currentHealth <= 0;
        public float GetHealthRatio => currentHealth / maxHealth.AsFloat();
		public int GetCurrentHealth => currentHealth;
        public bool IsDown => false;
        public int GetMaxHealth => maxHealth;

        public UpdateMode LeviathanUpdateMode => UpdateMode.LateUpdate;

        public void ResetHealth() => currentHealth = maxHealth;

        public bool TryStun(float duration)
        {
            if (brain == null)
                return false;
            
            return brain.TryToStun(duration);
        }
    }
}
