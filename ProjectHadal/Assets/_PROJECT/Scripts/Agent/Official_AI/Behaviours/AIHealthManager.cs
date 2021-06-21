using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Utility;
using Button = NaughtyAttributes.ButtonAttribute;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, ILeviathanComponent
    {
        [SerializeField] int maxHealth;
        int currentHealth;
        AIBrain brain;
        Timer stunTimer;

        public void Initialise(AIBrain brain)
        {
            this.brain = brain;
            if (maxHealth <= 0) maxHealth = 1;
            ResetHealth();

            //! Stun Timer
            stunTimer = brain.Create_A_Timer()
                                .WithDuration(brain.stunDuration)
                                .WithOnCompleteEvent(CancelStun)
                                .WithShouldPersist(true);
            stunTimer.Pause();

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

        private void CancelStun()
        {
            stunTimer.Pause();
            brain.StopStun();
        }
        [Button("StunAI")]
        void IStunYou()
        {
            TryStun(999);
        }

        public bool TryStun(float duration)
        {
            if (brain == null)
                return false;
            stunTimer.Resume();
            return brain.TryToStun(duration);
        }

    }
}
