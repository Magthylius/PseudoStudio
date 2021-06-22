using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.AI.Graphics;
using Hadal.Networking;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Utility;
using Button = NaughtyAttributes.ButtonAttribute;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, ISlowable, IAmLeviathan, ILeviathanComponent
    {
        [SerializeField] int maxHealth;
        
        [Header("Slow Stacking Status")]
        [SerializeField, Range(0f, 1f)] private float slowPercentPerStack;
        [SerializeField, Min(0)] private int maxSlowStacks;
        [SerializeField, Range(0f, 1f)] private float maxSlowPercent;
        private int currentSlowStacks;


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
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_DEATH, null, SendOptions.SendReliable);
                Death();
            }
        }

        public bool TakeDamage(int damage)
        {
            currentHealth = (currentHealth - damage).Clamp0();
            $"AI health: {currentHealth}".Msg();
            return true;
        }

        public void Death()
        {
            $"Leviathan is unalive. Congrats!!!".Msg();
            Obj.SetActive(false);
            brain.GraphicsHandler.gameObject.SetActive(false);
            brain.DetachAnyCarriedPlayer();
        }
        
        public GameObject Obj => transform.parent.gameObject;
        public bool IsUnalive => currentHealth <= 0;
        public float GetHealthRatio => currentHealth / maxHealth.AsFloat();
        public int GetCurrentHealth => currentHealth;
        public bool IsDown => false;
        public int GetMaxHealth => maxHealth;

        public UpdateMode LeviathanUpdateMode => UpdateMode.LateUpdate;

        public bool IsLeviathan => true;

        public void ResetHealth() => currentHealth = maxHealth;

        [Button("StunAI")]
        void IStunYou()
        {
            TryStun(5);
        }

        public bool TryStun(float duration)
        {
            if (!brain || brain.IsStunned)
                return false;
            
            Debug.LogWarning("stunned");
            stunTimer.RestartWithDuration(duration);
            return brain.TryToStun(duration);
        }
        
        private void CancelStun()
        {
            Debug.LogWarning("unstunned");
            stunTimer.Pause();
            brain.StopStun();
        }

        public void UpdateSlowStacks(int change)
        {
            currentSlowStacks = (currentSlowStacks + change);
            brain.NavigationHandler.SetSlowMultiplier(GetSlowPercentage());
            
            Debug.LogWarning("AI slowed to: " + GetSlowPercentage());
        }
        public void ResetAllSlowStacks() => currentSlowStacks = 0;
        public float GetSlowPercentage()
        {
            float percent = currentSlowStacks * slowPercentPerStack.AsFloat();
            if (percent > maxSlowPercent)
                percent = maxSlowPercent;
            return percent;
        }
    }
}
