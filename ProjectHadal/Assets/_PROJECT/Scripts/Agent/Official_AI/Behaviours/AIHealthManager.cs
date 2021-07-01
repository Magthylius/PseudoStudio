using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Utility;
using Button = NaughtyAttributes.ButtonAttribute;
using Photon.Pun;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, ISlowable, ILeviathanComponent
    {
        [Header("Health")]
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
            if (maxHealth <= 0) maxHealth = 1; //! max health must be at least 1 if we do not want the AI to die immediately on spawn
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

        /// <summary> Checks whether the AI should die on the master client's computer and send the death event over the network. </summary>
        public void CheckHealthStatus()
        {
            if (IsUnalive)
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_DEATH, null, SendOptions.SendReliable);
                Death();
            }
        }

        /// <summary>
        /// Only the AI can be actually damaged on the master client's computer. Other players will ask the master client to register the
        /// damage they do.
        /// </summary>
        public bool TakeDamage(int damage)
        {
            if (!PhotonNetwork.IsMasterClient)
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_DAMAGE, damage.Abs(), SendOptions.SendReliable);
            else
                currentHealth = (currentHealth - damage).Clamp0();
            
            return true;
        }

        /// <summary> Local death method to handle the AI death sequence & end the game. </summary>
        public void Death()
        {
            $"Leviathan is unalive. Congrats!!!".Msg();
            brain.GraphicsHandler.gameObject.SetActive(false);
            brain.DetachAnyCarriedPlayer();
            Obj.SetActive(false);
            
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

        public void ResetHealth() => currentHealth = maxHealth;

        [Button("StunAI")]
        void IStunYou()
        {
            TryStun(5);
        }

        /// <summary> Attempts to stun the AI for the given duration. </summary>
        /// <param name="duration">Custom stun duration.</param>
        /// <returns>Success if the AI can be and has been stunned.</returns>
        public bool TryStun(float duration)
        {
            if (!brain || brain.IsStunned)
                return false;
            
            stunTimer.RestartWithDuration(duration);
            return brain.TryToStun(duration);
        }
        
        /// <summary> Stops the stun effect and returns control to the AI. </summary>
        private void CancelStun()
        {
            stunTimer.Pause();
            brain.StopStun();
        }
		
        /// <summary> Meant for registering a projectile that can stack for a slow effect on the AI. </summary>
		public void AttachProjectile()
        {
            if (NetworkEventManager.Instance.IsMasterClient)
                UpdateSlowStacks(1);
            else
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, 1, SendOptions.SendReliable);
        }

        /// <summary> Meant for unregistering a projectile that can stack for a slow effect on the AI. </summary>
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
        /// <summary> Updates the current slow stacks on the AI and computes the slow percentage before sending it to the navigation handler. </summary>
        /// <param name="change">Change of stacks can be either positive or negative.</param>
        /// <param name="isLocal">If this function is called over the network (i.e. in a callback), this must be set to False.</param>
        public void UpdateSlowStacks(int change, bool isLocal = true)
        {
            currentSlowStacks = (currentSlowStacks + change).Clamp0();
            brain.NavigationHandler.SetSlowMultiplier(GetSlowPercentage());
			
			// if (isLocal)
            //     $"Updated Slow locally. Current stacks are {CurrentClampedSlowStacks} (exccess: {ExcessSlowStacks}); Max Velocity is now {brain.NavigationHandler.MaxVelocity}.".Msg();
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
            currentSlowPercent = currentSlowPercent.Clamp(0f, maxSlowPercent);
			return currentSlowPercent;
        }

        /// <summary> Current Total slow stacks on the AI. </summary>
		public int CurrentSlowStacks => currentSlowStacks;
        /// <summary> Returns the current total slow stacks clamped by the max allowed slow stacks value. </summary>
		public int CurrentClampedSlowStacks => currentSlowStacks > maxSlowStacks ? maxSlowStacks : currentSlowStacks;
        /// <summary> Returns the excess slow stacks over the max allowed amount if <see cref="CurrentSlowStacks"/> ever exceeds it. Otherwise, it will return 0. </summary>
		public int ExcessSlowStacks => currentSlowStacks > maxSlowStacks ? (currentSlowStacks - maxSlowStacks) : 0;
    }
}
