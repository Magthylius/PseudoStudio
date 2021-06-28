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
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, IAmLeviathan, ISlowable, ILeviathanComponent
    {
        [SerializeField] int maxHealth;
        
        [Header("Slow Stacking Status")]
        [SerializeField, Range(0f, 1f)] private float slowPercentPerStack;
        [SerializeField, Min(0)] private int maxSlowStacks;
        [SerializeField, Range(0f, 1f)] private float maxSlowPercent;
        [SerializeField, ReadOnly] private float currentMaxSlowPercent;
        [SerializeField, ReadOnly] private float currentSlowPercent;
        [SerializeField, ReadOnly] private int currentSlowStacks;

        int currentHealth;
        AIBrain brain;
        Timer stunTimer;

        private void OnValidate()
        {
            currentMaxSlowPercent = maxSlowStacks * slowPercentPerStack.AsFloat();
        }

        public void Initialise(AIBrain brain)
        {
            this.brain = brain;
            if (maxHealth <= 0) maxHealth = 1;
            ResetHealth();
            ResetAllSlowStacks();

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
            
            //! End the game
            GameManager.Instance.EndGameEvent();
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
            
            //Debug.LogWarning("stunned");
            stunTimer.RestartWithDuration(duration);
            return brain.TryToStun(duration);
        }
        
        private void CancelStun()
        {
            //Debug.LogWarning("unstunned");
            stunTimer.Pause();
            brain.StopStun();
        }
		
		public void AttachProjectile()
        {
            if (NetworkEventManager.Instance.IsMasterClient)
                UpdateSlowStacks(1);
            else
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, 1, SendOptions.SendReliable);
        }

        public void DetachProjectile()
        {
            if (NetworkEventManager.Instance.IsMasterClient)
                UpdateSlowStacks(-1);
            else
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, -1, SendOptions.SendReliable);
        }

        public void SetSlowStacks(int value)
        {
            currentSlowStacks = value;
            brain.NavigationHandler.SetSlowMultiplier(GetSlowPercentage());
        }
        public void UpdateSlowStacks(int change, bool isLocal = true)
        {
            currentSlowStacks = (currentSlowStacks + change).Clamp0();
            brain.NavigationHandler.SetSlowMultiplier(GetSlowPercentage());
			
			if (isLocal)
				$"Updated Slow locally. Current stacks are {CurrentSlowStacks}; Max Velocity is now {brain.NavigationHandler.MaxVelocity}.".Msg();
        }
        public void ResetAllSlowStacks()
        {
            currentSlowStacks = 0;
            currentMaxSlowPercent = maxSlowStacks * slowPercentPerStack.AsFloat();
            currentSlowPercent = 0;
        }

        public float GetSlowPercentage()
        {
            currentSlowPercent = currentSlowStacks * slowPercentPerStack.AsFloat();
            return currentSlowPercent.Clamp(0f, maxSlowPercent);
        }
		
		public int CurrentSlowStacks => currentSlowStacks;
    }
}
