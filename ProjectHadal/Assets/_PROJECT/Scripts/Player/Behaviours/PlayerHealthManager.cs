﻿using Photon.Pun;
using UnityEngine;
using Tenshi;
using System;
using System.Collections;
using ExitGames.Client.Photon;
using Hadal.Networking;
using System.Linq;
using Tenshi.UnitySoku;
using NaughtyAttributes;
using ReadOnly = Tenshi.ReadOnlyAttribute;
using System.Collections.Generic;
using Hadal.Utility;
using Hadal.AudioSystem;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IUnalivable, IKnockable, IInteractable, IPlayerComponent
    {
        #region Variable Declarations

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        [SerializeField] private bool redErrorsAsDamageSignal;

        [Header("Lifeline Settings")]
        [SerializeField] private bool enableDeathTimerWhenDown;
        [SerializeField, Tooltip("In seconds. Only applicable with death timer is enabled.")] private float deathTime;
        [SerializeField, ReadOnly] private float _deathTimer;

        [Space(10)]
        [SerializeField, Tooltip("In seconds. This timer is used to revive other players.")] private float reviveOtherTime;
        [SerializeField, ReadOnly] private float _localReviveTimer;
		[SerializeField, ReadOnly] private float _localReviveDelayTimer;
        [SerializeField] private float minOtherPlayerRevivalDistance;
		[SerializeField] private float localReviveDelayTime = 2f;
        [SerializeField] private float reviveOtherHealthPercent;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth;
        [SerializeField, ReadOnly] private int _currentHealth;
        private bool _isDead;
        private bool _isKnocked;
        private float _knockTimer;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerCameraController _cameraController;
        private bool _isKami;
        private bool _initialiseOnce;
        private bool _shouldRevive;
		private bool _canBeRevivedByOthers;
		
		public float ReviveTimeRatio { get; private set; } = 0f;

        [Header ("Downed Audio")]
        [SerializeField] private AudioEventData downedAudio;
        private Timer downSoundTimer;
        [SerializeField] private float downSoundDuration;

        [Header("Effects")] 
        public PlayerEffectManager EffectManager;
        public float MaxDamageEffect = 20f;
        
        #endregion

        #region Events

        /// <summary> Event will trigger when the player is hit while not in god mode, down mode or dead. <see cref="TakeDamage"/> </summary>
        public event Action<int> OnHit;

        /// <summary> Event will trigger when the player becomes dead. </summary>
        public event Action OnDeath;

        /// <summary> Event will trigger when the player becomes down but not out. </summary>
        public event Action OnDown;

        /// <summary> Event will trigger when the player takes damage. </summary>
        public event Action<float> OnDamageTaken;
		
		/// <summary> Event will trigger everytime the revive timer is initiated, cancelled or finished.
        /// Returns true when starting to revive another player ; returns false when no longer reviving another player. </summary>
		public event Action<bool> OnLocalRevivingAPlayer;

        /// <summary> Event will trigger everytime it reports the result of a revival attempt from this player
        /// towards another player. Returns true if this player successfully revives the other player; returns
        /// false if unsuccessful. </summary>
        public event Action<bool> OnLocalReviveAttempt;

        /// <summary> Event will trigger when this local player is revived over the network. </summary>
        public event Action<bool> OnNetworkReviveAttempt;

        #endregion

        #region Unity/System Lifecycle

        private void Awake()
        {
            _isDead = false;
            _isKami = false;
			_canBeRevivedByOthers = true;
        }

        private void Start()
        {
            //! UI setup
            OnDown += UpdateDownUI;

            OnLocalRevivingAPlayer += TriggerStartJumpstart;
            OnLocalReviveAttempt += JumpstartAttempt;
            OnNetworkReviveAttempt += UpdateReviveUI;
            OnNetworkReviveAttempt += StopDownSound;
            OnDamageTaken += EffectManager.HandleDamageEffect;

            //initializing sound loop set up
            downSoundTimer = this.Create_A_Timer()
                           .WithDuration(this.downSoundDuration)
                           .WithOnCompleteEvent(PlayDownSound)
                           .WithShouldPersist(true);
            downSoundTimer.Pause();
        }

        private void OnDestroy()
        {
            OnDown = null;
            OnNetworkReviveAttempt = null;
            OnDamageTaken = null;
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
            StartCoroutine(InitialiseRoutine());
        }

        private IEnumerator InitialiseRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            ResetHealth();
            NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_HEALTH_UPDATE, Receive_HealthUpdate);
            NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_UPDATED_REVIVE_TIME, Receive_ReviveTimeUpdate);

            if (IsLocalPlayer)
            {
                NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_RECEIVE_DAMAGE, Receive_TakeDamage);

                yield return new WaitForSeconds(0.5f);
                Send_HealthUpdateStatus(false);
            }
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
            //_controller.UI.InvokeOnHealthChange(_currentHealth);
            _controller.SetIsDown(false);
        }

        #endregion

        #region Health Modifiers

        /// <summary>
        /// Makes the player take a specified amount of damage from anything. Damage will be account for if not in god mode, down mode or dead.
        /// </summary>
        public bool TakeDamage(int damage)
        {
            //! Only evaluate health on local player
            if (_isKami || IsDown || IsUnalive || !IsLocalPlayer) return false;
            _currentHealth = (_currentHealth - damage.Abs()).Clamp0();
            DoOnHitEffects(damage);

            if (debugEnabled)
            {
                string msg = $"Taking {damage} Damage!";
                if (redErrorsAsDamageSignal) msg.Error();
                else msg.Msg();
            }

            CheckHealthStatus();
            Send_HealthUpdateStatus(false);
            
            OnDamageTaken?.Invoke(Mathf.Clamp01(damage / MaxDamageEffect));
            return true;
        }

        /// <summary>
        /// Resets the current health to its max value. Will refresh the (IsDown == true) function detection:
        /// <see cref="DeactivateControllerSystem"/>
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _controller.UI.InvokeOnHealthChange(_currentHealth);
            ResetDeathTimer();
            
            if (IsLocalPlayer)
            {
                if (debugEnabled) "For local player: Subscribing deactivate function in the case of IsDown = true.".Msg();
                OnDown += DeactivateControllerSystem;
                OnDown += ActivateDownLoopSound;
            }
        }

        /// <summary>
        /// Sets current health value. Should only be used in network callbacks.
        /// Can be used to kill the player, but the hp checking event must be manually triggered after this function.
        /// </summary>
        private void NetOnly_SetHealthValue(int health)
        {
            _currentHealth = health.Clamp(0, maxHealth);
        }

        /// <summary>
        /// Safely sets current health value. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthValue(int health)
        {
            _currentHealth = health.Clamp(1, maxHealth);
        }

        /// <summary>
        /// Safely sets current health to a percentage of max health. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthToPercent(float percent)
        {
            percent = percent.Clamp01();
            Safe_SetHealthValue((maxHealth * percent).Round());
        }

        #endregion

        #region Misc Control Methods

        public void CheckHealthStatus()
        {
            if (IsDown)
            {
                if (debugEnabled)
                {
                    if (IsLocalPlayer)
                        "You are Down!".Msg();
                    else
                        "An ally is Down!".Msg();
                }
                OnDown?.Invoke();
                return;
            }
            if (IsUnalive)
            {
                if (debugEnabled)
                {
                    if (IsLocalPlayer)
                        "You are Dead!".Msg();
                    else
                        "An ally is Dead!".Msg();
                }
                if (IsLocalPlayer)
                {
                    OnDeath?.Invoke();
                    OnDeath = null;
                    Send_HealthUpdateStatus(false);
                    _controller.Die();
                }
            }
        }

        private void DoOnHitEffects(int damage)
        {
            if (damage <= 5)
                _cameraController.ShakeCamera(damage);
            else
                _cameraController.ShakeCameraLeviathan();

            OnHit?.Invoke(damage);
            _controller.UI.InvokeOnHealthChange(_currentHealth);
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

        private Coroutine reviveTimerRoutine = null;
        private List<PlayerController> allPlayersCache = new List<PlayerController>();
        private void UpdatePlayerCache(bool forceRefresh = false)
        {
            if (allPlayersCache.IsEmpty() || forceRefresh)
                allPlayersCache = FindObjectsOfType<PlayerController>().ToList();
            
            allPlayersCache.RemoveAll(p => p == null);
        }

        public void Interact(int viewID)
        {
            //! Only players on other machine should be able to send the event to the true networked player of this pView
            if (IsLocalPlayer)
                return;
            
            //! Can only attempt revival if status is down and not dead
            if (IsDown && !IsUnalive)
            {
                if (reviveTimerRoutine != null || !_canBeRevivedByOthers) //! cannot run two timers simultaneously
                    return;

                UpdatePlayerCache();
                PlayerController actorPlayer = allPlayersCache.Where(p => p.ViewID == viewID).SingleOrDefault();

                //! There are duplicate view IDs or missing player reference from network
                if (actorPlayer == null)
                {
                    UpdatePlayerCache(true);
                    return;
                }

                //! down people cannot revive
                if (actorPlayer.GetInfo.HealthManager.IsDownOrUnalive)
                    return;

                if (debugEnabled)
                    $"Player of {PlayerViewID} is being interacted by player of {viewID}. This action will attempt to revive {PlayerViewID}.".Msg();
                
                reviveTimerRoutine = StartCoroutine(StartLocalReviveTimer(actorPlayer));
            }
        }

        /// <summary> Sets the reviving time for this local player to revive other players. </summary> 
        public void SetReviveOtherTime(float newReviveOtherTime)
        {
            reviveOtherTime = newReviveOtherTime;
        }

        public void SetReviveOtherPercentAmount(float newRevivePercentAmount)
        {
            reviveOtherHealthPercent = newRevivePercentAmount;
        }
        #endregion

        #region Is Down / Revive Control Methods

        /// <summary> Method to manage settings for revival player stats if a "full revive" is not necessary. </summary>
        private void SetRevivalCustomisations(float revivePercentAmount)
        {
            Debug.LogError("Revival final" + revivePercentAmount);
            Safe_SetHealthToPercent(revivePercentAmount); //! Revive at x% hp?
            _controller.UI.InvokeOnHealthChange(_currentHealth);
        }

        /// <summary> This do be playing the downed Audio loop - Jin
        private void ActivateDownLoopSound()
        {
            //! Only the local player can be deactivated
            if (!IsLocalPlayer)
                return;

            Debug.LogError("Thats abit sussy");
            PlayDownSound();
            OnDown -= ActivateDownLoopSound;
        }

        private void PlayDownSound()
        {
            downedAudio.PlayOneShot2D();
            /*downedAudio.PlayOneShot(transform);*/
            downSoundTimer.RestartWithDuration(downSoundDuration);
        }

        private void StopDownSound(bool success)
        {
            if(success)
                downSoundTimer.Pause();
        }

        /// <summary>
        /// Deactivates movement & rotation system and simulate a "down but not out" condition. Will only be called on the local player.
        /// </summary>
        private void DeactivateControllerSystem()
        {
            //! Only the local player can be deactivated
            if (!IsLocalPlayer)
                return;

            if (debugEnabled)
                "Deactivating control system for local player.".Msg();
            OnDown -= DeactivateControllerSystem; //! Unsubscribe so it is only called "once per life"

            _controller.SetIsDown(true); //! Disable movement & rotation
            _controller.SetPhysicHighFriction(); //! Update physics settings
            _controller.GetInfo.Shooter.SetCanFire(false);

            Vector3 downwardForce = Vector3.down * 200.0f;
            _controller.GetInfo.Rigidbody.AddForce(downwardForce, ForceMode.Acceleration);

            if (enableDeathTimerWhenDown)
                StartCoroutine(StartDeathTimer());
        }

        /// <summary> Activates movement & rotation system for revival. Will only be called on the local player. </summary>
        private bool TryRestoreControllerSystem(float revivePercentAmount)
        {
            if (IsUnalive || !IsDown || !IsLocalPlayer)
                return false;

            if (debugEnabled)
                "Restoring control system for local player.".Msg();
            StopAllCoroutines();
            ResetHealth();
            Debug.LogError("Revival 2" + revivePercentAmount);
            SetRevivalCustomisations(revivePercentAmount);
            _isDead = false; //! Make sure this is false

            _controller.SetIsDown(false); //! Enable movement & rotation
            _controller.SetPhysicNormal(); //! Update physics settings
            _controller.GetInfo.Shooter.SetCanFire(true);
            return true;
        }

        /// <summary>
        /// Checks whether the player should revive from IsDown status (fails if already dead). Should only be used in network callbacks.
        /// Only works for the local player.
        /// </summary>
        private void NetOnly_EvaluateRevive(float revivePercentAmount)
        {
            if (!_shouldRevive || !IsLocalPlayer || IsUnalive)
                return;

            //! revive the Local player
            _shouldRevive = false;
            Debug.LogError("Revival 1" + revivePercentAmount);
            TryRestoreControllerSystem(revivePercentAmount);
            CheckHealthStatus();
            OnNetworkReviveAttempt?.Invoke(true);
            Send_HealthUpdateStatus(false); //! send revive message to non-local players
        }

        #endregion

        #region UI Methods

        void UpdateDownUI()
        {
            _controller.UI.ContextHandler.PlayerWentDown();
            
            //Debug.LogWarning($"update down ui");
            if (_controller.transform != LocalPlayerData.PlayerController.transform)
            {
                //Debug.LogWarning($"track player down");
                LocalPlayerData.PlayerController.UI.TrackPlayerDown(_controller.transform);
            }
        }

        /// <summary> Meant for the networked other player that is being revived. </summary>
        void UpdateReviveUI(bool attemptSucceeded)
        {
            //Debug.LogWarning("update revive ui called: " + attemptSucceeded);
            if (attemptSucceeded)
            {
                //Debug.LogWarning(_controller.ViewID + " Triggered revive attempt");
                _controller.UI.ContextHandler.PlayerRevived();
                
                if (_controller.transform != LocalPlayerData.PlayerController.transform)
                    LocalPlayerData.PlayerController.UI.TrackPlayerRevived(_controller.transform);
            }
        }

        /// <summary> Meant for the local player that is doing the reviving action. </summary>
        void TriggerStartJumpstart(bool startedRevive)
        {
            if (startedRevive)
            {
                Debug.LogWarning(_controller.ViewID + " Triggered jumpstart");
                _controller.UI.ContextHandler.StartJumpstart();
            }
        }
        
        /// <summary> Meant for the local player that is doing the reviving action. </summary>
        void JumpstartAttempt(bool success)
        {
            Debug.LogWarning(_controller.ViewID + " Triggered jumpstart attempt: " + success);
            if (success) _controller.UI.ContextHandler.SuccessJumpstart();
            else _controller.UI.ContextHandler.FailJumpstart();
        }

        #endregion
        
        #region Network Event Methods

        /// <summary>
        /// Receives damage from event, only the Local player can take damage through this function.
        /// </summary>
        private void Receive_TakeDamage(EventData data)
        {
            object[] content = data.CustomData.AsObjArray();

            int targetViewID = (int)content[0];
            bool allowedToBeDamaged = _pView.ViewID == targetViewID && IsLocalPlayer;
            if (allowedToBeDamaged)
            {
                int damage = (int)content[1];
                TakeDamage(damage);
                if (debugEnabled)
                    $"I am taking damage by AI over the network. Pls halp".Msg();
            }
        }

        /// <summary>
        /// Sends a health update event. The boolean, "sendToTrueLocalPlayer", should be properly assigned for the event's flow to work as intended.
        /// </summary>
        /// <param name="sendToTrueLocalPlayer">If true, the nature of the event will be Non-local player -> Local player.
        /// If false, the nature will be Local player -> Non-local player.</param>
        private void Send_HealthUpdateStatus(bool sendToTrueLocalPlayer, float otherPlayerReviveHealthPercent = 0.2f)
        {
            object[] content;
            if (sendToTrueLocalPlayer) //! Non-local to Local player
            {
                bool shouldRevive = _shouldRevive;
                content = new object[]
                {
                    _pView.ViewID,
                    sendToTrueLocalPlayer,
                    shouldRevive,
                    otherPlayerReviveHealthPercent
                };
                _shouldRevive = false; //! reset should revive per event sent

                if (debugEnabled)
                    $"Sending event from non-local player to local player on another computer. Should revive: {shouldRevive}.".Msg();
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

                if (debugEnabled)
                    $"Sending event from local player to non-local player on another computer. Health: {_currentHealth}; Is Dead: {_isDead}.".Msg();
            }
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_HEALTH_UPDATE, content, SendOptions.SendReliable);
        }

        /// <summary>
        /// Receives an event from another player's health manager, instructing it to perform different function based on the 
        /// "bool:sendToTrueLocalPlayer" parameter.
        /// If it is True, the event will evaluate assuming it is on the local player's computer; if it is False, it will evaluate
        /// assuming it is on a non-local player computer.
        /// </summary>
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

                if (debugEnabled)
                    $"Received event from another player's computer, evaluating for local player. Should revive: {_shouldRevive}".Msg();
                Debug.LogError("Revival 0" + (float)content[3]);
                NetOnly_EvaluateRevive((float)content[3]);
            }
            else //! Evaluate on Non-local player
            {
                int localPlayerCurHealth = content[2].AsInt();
                if (localPlayerCurHealth != _currentHealth)
                    NetOnly_SetHealthValue(localPlayerCurHealth); //! sync hp with local player's if needed
                TakeDamage(localPlayerCurHealth);

                bool isDead = content[3].AsBool();
                _isDead = isDead; // set is dead or not
                CheckHealthStatus();

                if (debugEnabled)
                    $"Received event from another player's computer, evaluating for non-local player. New health: {_currentHealth}; Is Dead: {_isDead}".Msg();
            }
        }


        private void Receive_ReviveTimeUpdate(EventData data)
        {
            object[] content = data.CustomData.AsObjArray();

            int receivedViewID = content[0].AsInt();
            if (_pView.ViewID != receivedViewID)
                return;

            SetReviveOtherTime(content[1].AsFloat());
            SetReviveOtherPercentAmount(content[2].AsFloat());
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
        private IEnumerator StartLocalReviveTimer(PlayerController player, bool reviveLocallyOnTimerReached = false)
        {
            //! Reference player controller information of both players
            PlayerControllerInfo thisPlayerInfo = _controller.GetInfo;
            PlayerControllerInfo otherPlayerInfo = player.GetInfo;
            Transform thisPTrans = _controller.GetTarget;
            Transform otherPTrans = player.GetTarget;

            //! Use the revival distance & time of the other player that is reviving this player
            float sqrMinPlayerRevivalDistance = otherPlayerInfo.HealthManager.MinOtherPlayerRevivalDistance.Sqr();
            float startingReviveTime = otherPlayerInfo.HealthManager.OtherPlayerReviveTime;

            //! Reset local revive timer for this player
            ResetLocalReviveTimer();

            //! Local function definitions
            float SqrDistanceBetweenPlayers() => (thisPTrans.position - otherPTrans.position).sqrMagnitude;
            bool OtherPlayerIsCloseEnoughToRevive() => SqrDistanceBetweenPlayers() < sqrMinPlayerRevivalDistance;
            void ResetLocalReviveTimer() => _localReviveTimer = startingReviveTime;

            //! Start revive start event for the other player
			ReviveTimeRatio = 0f;
			otherPlayerInfo.HealthManager.OnLocalRevivingAPlayer?.Invoke(true);
            
            while (OtherPlayerIsCloseEnoughToRevive())
            {
                Debug_RevivalTimerStatus();
                
                //! Elapse timer & update revival completion ratio
                bool timerReached = ElapseReviveTimer(_controller.DeltaTime) < 0f;
				ReviveTimeRatio = (1f - (_localReviveTimer / startingReviveTime)).Clamp01();
				
                //! Check if revival criteria is reached, then send revive event over network
				if (timerReached)
                {
                    //! Start local revive delay timer to make sure all other players if the same view ID on other computers
                    //  receive the revive event for this player
					StartCoroutine(StartLocalReviveDelayTimer());

                    //! Invoke revive attempt success for the other player
                    _shouldRevive = true;
					ReviveTimeRatio = 1f;
					otherPlayerInfo.HealthManager.OnLocalRevivingAPlayer?.Invoke(false);
                    otherPlayerInfo.HealthManager.OnLocalReviveAttempt?.Invoke(true);
					
                    //! Send message of revival to Local player of this photon view ID
					Send_HealthUpdateStatus(true, otherPlayerInfo.HealthManager.reviveOtherHealthPercent);
					
                    //! This will be true only for debugging local reviving
                    if (reviveLocallyOnTimerReached)
                        TryRestoreControllerSystem(reviveOtherHealthPercent);

                    if (debugEnabled)
                        $"Criteria for revival complete.".Msg();
                    
                    reviveTimerRoutine = null;
                    yield break;
                }
                yield return null;
            }

            //! This will run when the timer does not end & the other player goes too far from this player
			ReviveTimeRatio = 0f;
			ResetLocalReviveTimer();
			
            //! Invoke revive attempt failure for the other player
            otherPlayerInfo.HealthManager.OnLocalRevivingAPlayer?.Invoke(false);
			otherPlayerInfo.HealthManager.OnLocalReviveAttempt?.Invoke(false);

            if (debugEnabled)
                $"Revival attempt failed.".Msg();
            
            reviveTimerRoutine = null;
        }
		
		private IEnumerator StartLocalReviveDelayTimer()
		{
			_canBeRevivedByOthers = false;
			_localReviveDelayTimer = localReviveDelayTime;
			
			while (_localReviveDelayTimer > 0f)
			{
				_localReviveDelayTimer -= _controller.DeltaTime;
				yield return null;
			}
			
			_canBeRevivedByOthers = true;
		}

        // private int screenLogReviveTimerIndex;
        // private DebugManager dManager;
        private void Debug_InitialiseRevivalTimerScreenLogger()
        {
            // dManager = DebugManager.Instance;
            // screenLogReviveTimerIndex = dManager.CreateScreenLogger();
        }

        private void Debug_RevivalTimerStatus()
        {
            // int seconds = (int)(_reviveTimer % 60);
            // dManager.SLog(screenLogReviveTimerIndex, $"Player Revive Timer (View id: {PlayerViewID}): ", seconds);
        }

        private float TickKnockTimer(in float deltaTime) => _knockTimer -= deltaTime;
        private void ResetKnockTimer(in float time) => _knockTimer = time;
        private void ResetDeathTimer() => _deathTimer = deathTime;
        private float ElapseDeathTimer(in float deltaTime) => _deathTimer -= deltaTime;
        private float ElapseReviveTimer(in float deltaTime) => _localReviveTimer -= deltaTime;

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

        /// <summary> Sets the health manager state to the criteria for <see cref="IsDown"/> to be true (i.e. 0 hp). </summary>
        [Button(nameof(Debug_BecomeDownButNotOut))]
        public void Debug_BecomeDownButNotOut()
        {
            if (IsDown)
            {
                "No need to beat a dead (or is it???????) horse.".Msg();
                return;
            }

            Debug_SetCurrentHealth(1);
            TakeDamage(1);
            CheckHealthStatus();

            if (IsDown)
                "Player successfully made down.".Msg();
        }

        /// <summary> Sets the health manager state to the criteria for <see cref="IsUnalive"/> to be true (i.e. dead). </summary>
        [Button(nameof(Debug_BecomeUnalive))]
        private void Debug_BecomeUnalive()
        {
            if (IsUnalive)
            {
                "People are killed when they die, but after that they do not die when they are killed.".Msg();
                return;
            }

            Debug_SetCurrentHealth(1);
            TakeDamage(1);
            _isDead = true;
            CheckHealthStatus();

            if (IsUnalive)
                "Player successfully made dead.".Msg();
        }

        /// <summary> Sets the health manager state to the criteria for revival (i.e. from teammates' interaction). </summary>
        [Button(nameof(Debug_InstantReviveFromDown))]
        public void Debug_InstantReviveFromDown()
        {
            if (!IsDown && !IsUnalive)
            {
                "Try to make alive someone who is not unalive sounds like a plot to obtain godhood. Please no".Msg();
                return;
            }

            TryRestoreControllerSystem(reviveOtherHealthPercent);
            OnNetworkReviveAttempt?.Invoke(true);

            if (!IsDown && !IsUnalive)
                "Player successfully revived.".Msg();
        }

        /// <summary> Sets the health manager state to the criteria for revival after the intended timer (i.e. from teammates' interaction). </summary>
        [Button(nameof(Debug_LocalReviveWithTimer))]
        private void Debug_LocalReviveWithTimer()
        {
            if (!IsDown && !IsUnalive || reviveTimerRoutine != null)
            {
                "Try to make alive someone who is not unalive sounds like a plot to obtain godhood. Please no".Msg();
                return;
            }

            reviveTimerRoutine = StartCoroutine(StartLocalReviveTimer(_controller, true));
        }

        #endregion

        #region Shorthands & Interface Getters

        private int PlayerViewID => _pView.ViewID;
        private bool IsLocalPlayer => _pView.IsMine;
        public float MinOtherPlayerRevivalDistance => minOtherPlayerRevivalDistance;
        public float OtherPlayerReviveTime => reviveOtherTime;
        public GameObject Obj => gameObject;
        public float GetHealthRatio => _currentHealth / (float)maxHealth;
        public int GetCurrentHealth => _currentHealth;
        public int GetMaxHealth => maxHealth;
        public bool IsUnalive => _isDead;
        public bool IsDown => _currentHealth <= 0;
        public bool IsDownOrUnalive => IsDown || IsUnalive;
        public bool IsKnocked => _isKnocked;

        #endregion
    }
}