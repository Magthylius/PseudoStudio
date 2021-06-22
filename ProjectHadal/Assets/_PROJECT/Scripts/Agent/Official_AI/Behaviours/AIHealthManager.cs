using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
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
        }
        
        public GameObject Obj => transform.parent.gameObject;
        public bool IsUnalive => currentHealth <= 0;
        public float GetHealthRatio => currentHealth / maxHealth.AsFloat();
        public int GetCurrentHealth => currentHealth;
        public bool IsDown => false;
        public int GetMaxHealth => maxHealth;

        public UpdateMode LeviathanUpdateMode => UpdateMode.LateUpdate;

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
            
            //stunTimer.Resume();
            stunTimer.RestartWithDuration(duration);
            return brain.TryToStun(duration);
        }
        
        private void CancelStun()
        {
            stunTimer.Pause();
            brain.StopStun();
        }
    }
}
