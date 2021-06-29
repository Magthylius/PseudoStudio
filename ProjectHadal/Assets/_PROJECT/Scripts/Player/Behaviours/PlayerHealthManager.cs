﻿using Photon.Pun;
using UnityEngine;
using Hadal.UI;
using Tenshi;
using System;
using System.Collections;
using ExitGames.Client.Photon;
using Hadal.Networking;
using System.Linq;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IUnalivable, IKnockable, IInteractable, IPlayerComponent
    {
        #region Variable Declarations

        [Header("Lifeline Settings")]
        [SerializeField] private bool enableDeathTimerWhenDown;
        [SerializeField, Tooltip("In seconds. Only applicable with death timer is enabled.")] private float deathTime;
        [SerializeField, ReadOnly] private float _deathTimer;
        
        [Space(10)]
        [SerializeField, Tooltip("In seconds. Only applicable when IsDown is true.")] private float reviveTime;
        [SerializeField, ReadOnly] private float _reviveTimer;
        [SerializeField] private float minPlayerRevivalDistance;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth;
        private int _currentHealth;
        private bool _isDead;
        private bool _isKnocked;
        private float _knockTimer;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerCameraController _cameraController;
        private bool _isKami;
        private bool _initialiseOnce;
        private bool _shouldRevive;

        #endregion

        #region Events

        /// <summary> Event will trigger when the player is hit while not in god mode, down mode or dead. <see cref="TakeDamage"/> </summary>
        public event Action<int> OnHit;
        
        /// <summary> Event will trigger when the player becomes dead. </summary>
        public event Action OnDeath;

        /// <summary> Event will trigger when the player becomes down but not out. </summary>
        public event Action OnDown;

        /// <summary> Event will trigger everytime it reports the result of a revival attempt on this player.
        /// Returns true if a revival attempt was successful; returns false if unsuccessful. </summary>
        public event Action<bool> OnReviveAttempt;

        #endregion

        #region Unity/System Lifecycle

        private void Awake()
        {
            _isDead = false;
            _isKami = false;
        }
        private void OnDestroy()
        {
            OnDown = null;
        }
        private void OnValidate()
        {
            if (maxHealth <= 0) maxHealth = 1;
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _controller = controller;
            _pView = info.PhotonInfo.PView;
            _cameraController = info.CameraController;
			ResetHealth();

            if (!_initialiseOnce)
            {
                _initialiseOnce = true;
                PlayerController.OnInitialiseComplete += Initialise;
                Debug_InitialiseRevivalTimerScreenLogger();
            }
        }

        /// <summary>
        /// Is meant to be called when the associated player controller has finished initialisation. It makes all player controller listen
        /// to health update events over the network.
        /// </summary>
        private void Initialise(PlayerController player)
        {
            if (_controller != player)
                return;

            PlayerController.OnInitialiseComplete -= Initialise;
            NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_HEALTH_UPDATE, Receive_HealthUpdate);
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!_isKnocked)
                return;

            if (TickKnockTimer(deltaTime) <= 0f)
                _isKnocked = false;
        }

        public void ResetManager()
        {
            ResetHealth();
            UIManager.InvokeOnHealthChange();
            _controller.SetIsDown(false);
        }

        #endregion

        #region Health Modifiers

        /// <summary>
        /// Makes the player take a specified amount of damage from anything. Damage will be account for if not in god mode, down mode or dead.
        /// </summary>
        public bool TakeDamage(int damage)
        {
            if (_isKami || IsDown || IsUnalive) return false;
            _currentHealth = (_currentHealth - damage.Abs()).Clamp0();
            DoOnHitEffects(damage);
            if (IsLocalPlayer) //! Only evaluate health status on local player
            {
                CheckHealthStatus();
                Send_HealthUpdateStatus(false);
            }
            return true;
        }

        /// <summary>
        /// Resets the current health to its max value. Will refresh the (IsDown == true) function detection:
        /// <see cref="DeactivateControllerSystem"/>
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            ResetDeathTimer();
            ResetReviveTimer();
            if (IsLocalPlayer) OnDown += DeactivateControllerSystem;
        }

        /// <summary>
        /// Sets current health value. Should only be used in network callbacks.
        /// Can be used to kill the player, but the hp checking event must be manually triggered after this function.
        /// </summary>
        private void NetOnly_SetHealthValue(in int health)
        {
            _currentHealth = health.Clamp(0, maxHealth);
        }

        /// <summary>
        /// Safely sets current health value. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthValue(in int health)
        {
            _currentHealth = health.Clamp(1, maxHealth);
        }

        /// <summary>
        /// Safely sets current health to a percentage of max health. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthToPercent(float percent)
        {
            percent = percent.Clamp01();
            Safe_SetHealthValue((maxHealth * percent).AsInt());
        }

        #endregion

        #region Misc Control Methods

        public void CheckHealthStatus()
        {
            if (IsDown)
            {
                OnDown?.Invoke();
                return;
            }
            if (IsUnalive)
            {
                OnDeath?.Invoke();
                _controller.Die();
                Send_HealthUpdateStatus(false);
            }
        }

        private void DoOnHitEffects(int damage)
        {
            _cameraController.ShakeCameraDefault();
            OnHit?.Invoke(damage);
            UIManager.InvokeOnHealthChange();
        }

        #endregion

        #region Knockback/Interaction Methods

        public bool TryToKnock(Vector3 force, float duration)
        {
            if (_isKnocked)
                return false;

            _isKnocked = true;
            ResetKnockTimer(duration);
            _controller.GetInfo.Rigidbody.AddForce(force, ForceMode.Impulse);
            return true;
        }

        public void Interact(int viewID)
        {
            //! Only players on other machine should be able to send the event to the true networked player of this pView
            if (!IsLocalPlayer)
                return;
            
            //! Can only attempt revival if status is down and not dead
            if (IsDown && !IsUnalive)
            {
                if (ReviveTimerIsRunning())
                    return;
                
                PlayerController actorPlayer = NetworkEventManager.Instance.PlayerObjects
                                                .Select(p => p.GetComponent<PlayerController>())
                                                .Where(p => p.GetInfo.PhotonInfo.PView.ViewID == viewID)
                                                .SingleOrDefault();
                if (actorPlayer == null) //! Duplicate view IDs or missing player reference
                    return;

                StartCoroutine(StartReviveTimer(actorPlayer));
            }
        }

        #endregion

        #region Is Down / Revive Control Methods

        /// <summary> Method to manage settings for revival player stats if a "full revive" is not necessary. </summary>
        private void SetRevivalCustomisations()
        {
            Safe_SetHealthToPercent(0.5f); //! Revive at x% hp?
        }

        /// <summary>
        /// Deactivates movement & rotation system and simulate a "down but not out" condition. Will only be called on the local player.
        /// </summary>
        private void DeactivateControllerSystem()
        {
            if (!IsLocalPlayer)
                return;
            OnDown -= DeactivateControllerSystem; //! Unsubscribe so it is only called "once per life"

            _controller.SetIsDown(true); //! Disable movement & rotation
            _controller.SetPhysicHighFriction(); //! Update physics settings

            if (enableDeathTimerWhenDown)
                StartCoroutine(StartDeathTimer());
        }

        /// <summary>
        /// Activates movement & rotation system for revival. Will only be called on the local player.
        /// </summary>
        private bool TryRestoreControllerSystem()
        {
            if (IsUnalive || !IsDown)
                return false;

            StopAllCoroutines();
            ResetHealth();
            SetRevivalCustomisations();
            _isDead = false; //! Make sure this is false

            _controller.SetIsDown(false); //! Enable movement & rotation
            _controller.SetPhysicNormal(); //! Update physics settings
            return true;
        }

        /// <summary>
        /// Checks whether the player should revive from IsDown status (fails if already dead). Should only be used in network callbacks.
        /// Only works for the local player.
        /// </summary>
        private void NetOnly_EvaluateRevive()
        {
            if (!_shouldRevive || !IsLocalPlayer || IsUnalive)
                return;
            
            //! revive the Local player
            _shouldRevive = false;
            TryRestoreControllerSystem();
            CheckHealthStatus();
            Send_HealthUpdateStatus(false); //! send revive message to non-local players
        }

        #endregion

        #region Network Event Methods

        /// <summary>
        /// Sends a health update event. The boolean, "sendToTrueLocalPlayer", should be properly assigned for the event's flow to work as intended.
        /// </summary>
        /// <param name="sendToTrueLocalPlayer">If true, the nature of the event will be Non-local player -> Local player.
        /// If false, the nature will be Local player -> Non-local player.</param>
        private void Send_HealthUpdateStatus(bool sendToTrueLocalPlayer)
        {
            //! Only the local player should be able to send the event to report to all the simulated networked players
            if (!IsLocalPlayer)
                return;
            
            object[] content;
            if (sendToTrueLocalPlayer) //! Non-local to Local player
            {
                bool shouldRevive = _shouldRevive;
                content = new object[]
                {
                    _pView.ViewID,
                    sendToTrueLocalPlayer,
                    shouldRevive
                };
                _shouldRevive = false; //! reset should revive per event sent
            }
            else //! Local player to Non-local
            {
                content = new object[]
                {
                    _pView.ViewID,
                    sendToTrueLocalPlayer,
                    _currentHealth,
                    _isDead
                };
            }
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_HEALTH_UPDATE, content, SendOptions.SendReliable);
        }

        private void Receive_HealthUpdate(EventData data)
        {
            object[] content = data.CustomData.AsObjArray();
            
            int receivedViewID = content[0].AsInt();
            if (_pView.ViewID != receivedViewID)
                return;

            bool sendToTrueLocalPlayer = content[1].AsBool();

            if (sendToTrueLocalPlayer) //! Evaluate on Local player
            {
                bool shouldRevive = content[2].AsBool();
                _shouldRevive = shouldRevive;
                NetOnly_EvaluateRevive();
            }
            else //! Evaluate on Non-local player
            {
                int localPlayerCurHealth = content[2].AsInt();
                if (localPlayerCurHealth != _currentHealth)
                    NetOnly_SetHealthValue(localPlayerCurHealth); //! sync hp with local player's if needed

                bool isDead = content[3].AsBool();
                _isDead = isDead; // set is dead or not
                CheckHealthStatus();
            }
        }

        #endregion

        #region Timer Control Methods
        
        /// <summary> Coroutine for assessing player death. </summary>
        private IEnumerator StartDeathTimer()
        {
            ResetDeathTimer();
            while (IsDown)
            {
                if (ElapseDeathTimer(_controller.DeltaTime) < 0f)
                {
                    //! Player is officially unalive
                    _isDead = true;
                    CheckHealthStatus();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary> Coroutine for assessing player revival. </summary>
        private IEnumerator StartReviveTimer(PlayerController player)
        {
            ResetReviveTimer();
            Transform thisPTrans = _controller.GetTarget;
            Transform otherPTrans = player.GetTarget;
            float sqrMinPlayerRevivalDistance = minPlayerRevivalDistance.Sqr();
            
            float SqrDistanceBetweenPlayers() => (thisPTrans.position - otherPTrans.position).sqrMagnitude;
            bool OtherPlayerIsCloseEnoughToRevive() => SqrDistanceBetweenPlayers() < sqrMinPlayerRevivalDistance;

            while (OtherPlayerIsCloseEnoughToRevive())
            {
                Debug_RevivalTimerStatus();
                if (ElapseReviveTimer(_controller.DeltaTime) < 0f)
                {
                    //! Revivalllllllllll
                    _shouldRevive = true;
                    Send_HealthUpdateStatus(true); //! send message of revival to Local player
                    OnReviveAttempt?.Invoke(true);
                    yield break;
                }
                yield return null;
            }

            //! This will run when the timer does not end & the other player goes too far from this player
            ResetReviveTimer();
            OnReviveAttempt?.Invoke(false);
        }

        private int screenLogReviveTimerIndex;
        private DebugManager dManager;
        private void Debug_InitialiseRevivalTimerScreenLogger()
        {
            dManager = DebugManager.Instance;
            screenLogReviveTimerIndex = dManager.CreateScreenLogger();
        }

        private void Debug_RevivalTimerStatus()
        {
            int seconds = (_reviveTimer % 60).AsInt();
            dManager.SLog(screenLogReviveTimerIndex, $"Player Revive Timer (View id: {PlayerViewID}): ", seconds);
        }

        private float TickKnockTimer(in float deltaTime) => _knockTimer -= deltaTime;
        private void ResetKnockTimer(in float time) => _knockTimer = time;
        private void ResetDeathTimer() => _deathTimer = deathTime;
        private float ElapseDeathTimer(in float deltaTime) => _deathTimer -= deltaTime;
        private bool ReviveTimerIsRunning() => _reviveTimer < reviveTime && _reviveTimer > 0f;
        private void ResetReviveTimer() => _reviveTimer = reviveTime;
        private float ElapseReviveTimer(in float deltaTime) => _reviveTimer -= deltaTime;
        
        #endregion

        #region Debug Methods

        /// <summary> Set current health to value for debugging purposes. Values below zero are ignored. Will be affected by god mode. </summary>
        public void Debug_SetCurrentHealth(int health)
        {
            _currentHealth = health.Clamp0();
            TakeDamage(0);
        }

        /// <summary> God mode for debugging purposes. Immunity to damage & death. </summary>
        public void Debug_SetGodMode(bool statement) => _isKami = statement;
        public void Debug_ToggleGodMode() => Debug_SetGodMode(!_isKami);

        #endregion

        #region Shorthands & Interface Getters

        private int PlayerViewID => _pView.ViewID;
        private bool IsLocalPlayer => _pView.IsMine;
        public GameObject Obj => gameObject;
        public float GetHealthRatio => _currentHealth / (float)maxHealth;
        public int GetCurrentHealth => _currentHealth;
        public int GetMaxHealth => maxHealth;
        public bool IsUnalive => _isDead;
        public bool IsDown => _currentHealth <= 0;
        public bool IsKnocked => _isKnocked;

        #endregion
    }
}