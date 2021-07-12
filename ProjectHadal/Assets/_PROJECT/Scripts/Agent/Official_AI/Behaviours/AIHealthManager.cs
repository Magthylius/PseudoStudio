using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Utility;
using Button = NaughtyAttributes.ButtonAttribute;
using Photon.Pun;
using System.Linq;
using Hadal.AI.Graphics;

namespace Hadal.AI
{
    public class AIHealthManager : MonoBehaviour, IDamageable, IUnalivable, IStunnable, ISlowable, ILeviathanComponent
    {
        [Header("Health")]
        [SerializeField] int maxHealth;
        [SerializeField, ReadOnly] int currentHealth;
        
        [Header("Slow Stacking Status")]
        [SerializeField, Range(0f, 1f)] private float slowPercentPerStack;
        [SerializeField, Min(0)] private int maxSlowStacks;
        [SerializeField, Range(0f, 1f)] private float maxSlowPercent;
        [SerializeField, ReadOnly] private float currentMaxSlowPercent;
        [SerializeField, ReadOnly] private float currentSlowPercent;
        [SerializeField, ReadOnly] private int currentSlowStacks;

        [Header("VFX")]
        [SerializeField, ReadOnly, Tooltip("This will reference the graphics handler's hit positions.")] private Transform[] randomHitPoints;
        [SerializeField] private VFXData vfx_OnDamaged;
        [SerializeField] private int vfxCountPerHit = 4;
        [SerializeField] private int vfxCountPerDeath = 20;

        AIBrain brain;
        Timer stunTimer;
        private bool _killedWithCheat = false;

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

            randomHitPoints = brain.GraphicsHandler.potentialHitPositions.ToArray();
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
            _killedWithCheat = damage == int.MaxValue;

            if (!PhotonNetwork.IsMasterClient)
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_DAMAGE, damage.Abs(), SendOptions.SendReliable);
            else
                currentHealth = (currentHealth - damage).Clamp0();
            
            brain.RuntimeData.AddDamageCount();
            DoOnHitEffects(damage);

            return true;
        }

        private bool checkHitWallOnDeath = false;
        /// <summary> Local death method to handle the AI death sequence & end the game. </summary>
        public void Death()
        {
            string extraMsg = string.Empty;
            if (_killedWithCheat)
                extraMsg = " ..., you sure you did not cheat???".Bold();
            
            $"Leviathan is unalive. Congrats!!!{extraMsg}".Msg();
            
            // AIGraphicsHandler gHandler = brain.GraphicsHandler;
            // if (gHandler == null) gHandler = FindObjectOfType<AIGraphicsHandler>();
            // gHandler.gameObject.SetActive(false);

            brain.DetachAnyCarriedPlayer();
            brain.DisableBrain(); //! disable update loops of the brain
            brain.StartCoroutine(Bleed(0.5f));
            if (PhotonNetwork.IsMasterClient)
                brain.NavigationHandler.DisableWithLerp(2f, Sink);
            
            void Sink()
            {
                checkHitWallOnDeath = true;
                Vector3 force = Vector3.down * 1000.0f;
                brain.NavigationHandler.Rigidbody.isKinematic = false;
                brain.NavigationHandler.Rigidbody.AddForce(force, ForceMode.Acceleration);
            }

            //! Handle End the game
            brain.GameHandler.AILoseGame();

            System.Collections.IEnumerator Bleed(float bleedDelay)
            {
                //! Burst bleed
                int count = -1;
                while (++count < vfxCountPerDeath)
                    PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
                
                //! Continuous bleed over time (until game exits to main menu)
                WaitForSeconds waitTime = new WaitForSeconds(bleedDelay > 0f ? bleedDelay : 0.5f);
                while (true)
                {
                    PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
                    yield return waitTime;
                }
            }
        }

        public System.Func<bool> OnCollisionDetected() => () =>
        {
            if (!checkHitWallOnDeath || !IsUnalive)
                return false;

            Rigidbody rBody = brain.NavigationHandler.Rigidbody;
            if (rBody != null)
            {
                rBody.isKinematic = true;
                rBody.velocity = Vector3.zero;
            }

            return true;
        };
        
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
            float duration = 5f;
            TryStun(duration);
        }

        /// <summary> Attempts to stun the AI for the given duration. </summary>
        /// <param name="duration">Custom stun duration.</param>
        /// <returns>Success if the AI can be and has been stunned.</returns>
        public bool TryStun(float duration)
        {
            if (!brain || brain.IsStunned)
                return false;
            
            stunTimer.RestartWithDuration(duration);
            SendStunEvent();
            return brain.TryToStun(duration);

            void SendStunEvent()
                => NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_RECEIVE_STUN, duration, SendOptions.SendReliable);
        }

        /// <summary> Same as the normal TryStun but this should always be called on network event callbacks. </summary>
        public void Receive_TryStun(float duration)
        {
            if (!brain || brain.IsStunned)
                return;
            
            stunTimer.RestartWithDuration(duration);
            brain.TryToStun(duration);
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

        /// <summary> On hit effects should be stuffed into this function. VFX, audio, etc. </summary>
        private void DoOnHitEffects(int damageReceived)
        {
            if (randomHitPoints.IsNotEmpty())
            {
                int i = -1;
                while (++i < vfxCountPerHit)
                    PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
            }
        }

        private Vector3 GetRandomHitPosition() => randomHitPoints.RandomElement().position;

        /// <summary> A wrapper function that automatically handles null reference cases, just call this function with no worries. </summary>
        private void PlayVFXAt(VFXData vfx, Vector3 position)
        {
            if (vfx == null) return;
            vfx.SpawnAt(position);
        }
    }
}
