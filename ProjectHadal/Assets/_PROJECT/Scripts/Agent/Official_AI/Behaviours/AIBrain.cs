using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Hadal.AI.States;
using Hadal.AI.Caverns;
using Photon.Pun;
using System.Linq;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Player;
using Hadal.AI.Graphics;
using Hadal.Networking;
using ExitGames.Client.Photon;

namespace Hadal.AI
{
    public delegate void PhaseEvents(bool isStarting);

    public class AIBrain : MonoBehaviour, IAmLeviathan
    {
        [ReadOnly, SerializeField] private bool isEnabled = true;
        [ReadOnly, SerializeField] private bool onMasterClient;

        [Header("Read-only data")]
        [ReadOnly, SerializeField] private CavernHandler targetMoveCavern;
        [ReadOnly, SerializeField] private CavernHandler nextMoveCavern;
        [ReadOnly, SerializeField] private CavernHandler cachedCurrentCavern;

        [Header("Module Components")]
        [SerializeField] private AIHealthManager healthManager;
        [SerializeField] private PointNavigationHandler navigationHandler;
        [SerializeField] private AISenseDetection senseDetection;
        [SerializeField] private AISightDetection sightDetection;
        [SerializeField] private AIDamageManager damageManager;
        [SerializeField] private AIGameHandler gameHandler;
        [SerializeField] private AIAudioBank audioBank;
        [SerializeField] private AIGraphicsHandler graphicsHandler;
        [SerializeField] private CavernManager cavernManager;
		[SerializeField] private AIEmissiveColor emissiveColor;
		[SerializeField] private AIAnimationManager animationManager;
        private NetworkEventManager neManager;
        public AIHealthManager HealthManager => healthManager;
        public PointNavigationHandler NavigationHandler => navigationHandler;
        public AISenseDetection SenseDetection => senseDetection;
        public AISightDetection SightDetection => sightDetection;
        public AIDamageManager DamageManager => damageManager;
        public AIGameHandler GameHandler => gameHandler;
        public AIAudioBank AudioBank => audioBank;
        public AIGraphicsHandler GraphicsHandler => graphicsHandler;
        public CavernManager CavernManager => cavernManager;
		public AIEmissiveColor EmissiveColor => emissiveColor;
		public AIAnimationManager AnimationManager => animationManager;

        private StateMachine stateMachine;
        private List<ILeviathanComponent> allAIUpdateComponents;
        private List<ILeviathanComponent> preUpdateComponents;
        private List<ILeviathanComponent> mainUpdateComponents;

        [Header("Runtime Data")]
        [SerializeField] private LeviathanRuntimeData runtimeData;
        [ReadOnly] public GameObject MouthObject;
        [ReadOnly] public List<PlayerController> Players;
        [ReadOnly] public AIEgg Egg;
        [SerializeField, ReadOnly] private PlayerController currentTarget;
        public PlayerController CurrentTarget { get => currentTarget; private set => currentTarget = value; }
        /// <summary> Allows safely setting the current target of the AI if it is not already in Judgement state. </summary>
        public void TrySetCurrentTarget(PlayerController newTarget)
        {
            if (RuntimeData.GetBrainState == BrainState.Judgement)
                return;

            CurrentTarget = newTarget;
        }
        /// <summary> Unsafely sets the current target. Can be intentionally used when switching targets midway is necessary. </summary>
        public void ForceSetCurrentTarget(PlayerController newTarget) => CurrentTarget = newTarget;
        /// <summary> Network callback only version of <see cref="TrySetCurrentTarget"/> that bypasses the safety check. </summary>
        public void Net_SetCurrentTarget(PlayerController newTarget) => ForceSetCurrentTarget(newTarget);

        private PlayerController carriedPlayer;

        //[ReadOnly]
        public PlayerController CarriedPlayer
        {
            get { return carriedPlayer; }
            set
            {
                carriedPlayer = value;
                if (DebugEnabled) Debug.LogWarning("Carried player changed into: " + value);
            }
        }

        [Header("Settings Data")]
        [SerializeField] private StateMachineData machineData;
        [SerializeField] private bool followNetworkManagerOfflineStatus;
        [SerializeField] private bool isOffline;
        public bool DebugEnabled;

        public LeviathanRuntimeData RuntimeData => runtimeData;
        public StateMachineData MachineData => machineData;
        private Rigidbody rBody;

        AIStateBase anticipationState;
        AIStateBase ambushState;
        AIStateBase huntState;
        AIStateBase judgementState;
        AIStateBase recoveryState;
        AIStateBase cooldownState;
        List<AIStateBase> allStates;

        [Header("Stunned Settings")]
        [SerializeField, ReadOnly] bool isStunned;
        public float stunDuration;
        public event Action<bool> OnStunnedEvent;

        private bool _playersAreReady;

        private void Awake()
        {
            if (!isEnabled || isOffline) return;

            _playersAreReady = false;
            rBody = GetComponent<Rigidbody>();
            graphicsHandler = FindObjectOfType<AIGraphicsHandler>();
            isStunned = false;

            allAIUpdateComponents = GetComponentsInChildren<ILeviathanComponent>()
                .Where(c => c.LeviathanUpdateMode != UpdateMode.DoNotUpdate)
                .ToList();
            preUpdateComponents = allAIUpdateComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.PreUpdate).ToList();
            mainUpdateComponents = allAIUpdateComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.MainUpdate).ToList();

            Players = new List<PlayerController>();
            CurrentTarget = null;
            CarriedPlayer = null;

            runtimeData.Awake_Initialise();
            navigationHandler.Initialise();
        }

        private void Start()
        {
            neManager = NetworkEventManager.Instance;
            if (neManager != null && followNetworkManagerOfflineStatus)
                isOffline = neManager.isOfflineMode;

            onMasterClient = PhotonNetwork.IsMasterClient || isOffline;
            if (!onMasterClient)
            {
                healthManager.Initialise(this);
				emissiveColor = FindObjectOfType<AIEmissiveColor>(); emissiveColor.Initialise(this, onMasterClient);
				animationManager = FindObjectOfType<AIAnimationManager>(); animationManager.Initialise(this, onMasterClient);
                neManager.AddListener(ByteEvents.AI_PLAY_AUDIO, Receive_PlayAudio);
                return;
            }
			
            if (!isEnabled) return;

            Setup();
            StartCoroutine(InjectAIDependencies());

            IEnumerator InjectAIDependencies()
            {
                while (LocalPlayerData.PlayerController == null)
                {
                    //Debug.LogWarning("waiting for player to init");
                    yield return null;
                }
                LocalPlayerData.PlayerController.InjectAIDependencies(transform);
            }
        }

        private void Update()
        {
            if (!onMasterClient) return;
            if (!CanUpdate || !isEnabled) return;
            float deltaTime = DeltaTime;
            preUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
            navigationHandler.DoUpdate(deltaTime);
            stateMachine?.MachineTick();
            mainUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
            HandleCarriedPlayer();
        }
        private void LateUpdate()
        {
            if (!onMasterClient) return;
            if (!CanUpdate || !isEnabled) return;
            stateMachine?.LateMachineTick();
            allAIUpdateComponents.ForEach(c => c.DoLateUpdate(DeltaTime));
        }
        private void FixedUpdate()
        {
            if (!onMasterClient) return;
            if (!CanUpdate || !isEnabled) return;
            float fixedDeltaTime = FixedDeltaTime;
            navigationHandler.DoFixedUpdate(fixedDeltaTime);
            stateMachine?.FixedMachineTick();
            allAIUpdateComponents.ForEach(c => c.DoFixedUpdate(fixedDeltaTime));
        }

        private void OnDestroy()
        {
            if (cavernManager != null)
            {
                cavernManager.AIEnterCavernEvent -= OnCavernEnter;
                cavernManager.AILeftCavernEvent -= OnCavernLeave;
                cavernManager.PlayerEnterCavernEvent -= OnPlayerEnterAICavern;
                cavernManager.AIEnterTunnelEvent -= OnTunnelEnter;
                cavernManager.AILeftTunnelEvent -= OnTunnelLeave;
            }
            if (Egg != null) Egg.eggDestroyedEvent -= HandleEggDestroyedEvent;
            if (!onMasterClient)
            {
                if (neManager != null)
                {
                    neManager.RemoveListener(ByteEvents.AI_PLAY_AUDIO, Receive_PlayAudio);
                }
            }
        }

        void Setup()
        {
            if (DebugEnabled && isOffline)
                "Leviathan brain initialising in Offline mode.".Msg();

            allAIUpdateComponents.ForEach(i => i.Initialise(this));
			emissiveColor = FindObjectOfType<AIEmissiveColor>(); emissiveColor.Initialise(this, onMasterClient);
            cavernManager = FindObjectOfType<CavernManager>();
			animationManager = FindObjectOfType<AIAnimationManager>(); animationManager.Initialise(this, onMasterClient);
            Egg = FindObjectOfType<AIEgg>();

            //! Event handling
            if (cavernManager != null)
            {
                cavernManager.AIEnterCavernEvent += OnCavernEnter;
                cavernManager.AILeftCavernEvent += OnCavernLeave;
                cavernManager.PlayerEnterCavernEvent += OnPlayerEnterAICavern;
                cavernManager.AIEnterTunnelEvent += OnTunnelEnter;
                cavernManager.AILeftTunnelEvent += OnTunnelLeave;
            }
            if (Egg != null) Egg.eggDestroyedEvent += HandleEggDestroyedEvent;

            PlayerManager pManager = PlayerManager.Instance;
            if (pManager != null && PhotonNetwork.IsMasterClient)
                pManager.OnAllPlayersReadyEvent += PlayersAreReadySignal;

            //! State machine
            InitialiseStates();
            if (!startWithOverrideState)
            {
                runtimeData.SetBrainState(BrainState.Anticipation);
                stateMachine.SetState(anticipationState);
            }
            else
            {
                runtimeData.SetBrainState(overrideState);
                stateMachine.SetState(GetMachineState(overrideState));
            }


            //! Runtime data
            RefreshPlayerReferences();
            runtimeData.Start_Initialise();
            navigationHandler.SetCavernManager(cavernManager);
            if (graphicsHandler != null) MouthObject = graphicsHandler.MouthObject;

        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();

            //! Anticipation
            anticipationState = new AnticipationState(this);

            //! Engagement
            ambushState = new AmbushState(this);
            huntState = new HuntState(this);
            judgementState = new JudgementState(this);

            //! Recovery
            recoveryState = new RecoveryState(this);

            //! Cooldown
            cooldownState = new CooldownState(this);

            //! -setup custom transitions-
            stateMachine.AddEventTransition(to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddEventTransition(to: judgementState, withCondition: CanJudge());
            stateMachine.AddEventTransition(to: ambushState, withCondition: WantsToAmbush());
            stateMachine.AddEventTransition(to: huntState, withCondition: WantsToHunt());
            stateMachine.AddEventTransition(to: recoveryState, withCondition: IsRecovering());
            stateMachine.AddEventTransition(to: cooldownState, withCondition: IsCooldown());

            allStates = new List<AIStateBase>
            {
                anticipationState,
                ambushState,
                huntState,
                judgementState,
                recoveryState,
                cooldownState
            };
        }

        private void PlayersAreReadySignal()
        {
            PlayerManager.Instance.OnAllPlayersReadyEvent -= PlayersAreReadySignal;
            if (DebugEnabled) "Players are ready, Happy hunting!".Msg();
            _playersAreReady = true;
        }

        private readonly Vector3 vZero = Vector3.zero;
        private void HandleCarriedPlayer()
        {
            if (CarriedPlayer == null) return;
            CarriedPlayer.GetTarget.localPosition = vZero;
        }

        #region Event Handlers

        /// <summary> Calls when AI enters a cavern. </summary>
        void OnCavernEnter(CavernHandler cavern)
        {
            GetCurrentMachineState().OnCavernEnter(cavern);
            UpdateCachedCurrentCavern(cavern);
            if (NavigationHandler != null)
            {
                var newTag = CachedCurrentCavern != null ? CachedCurrentCavern.cavernTag : CavernTag.Invalid;
                var nextTag = NextMoveCavern != null ? NextMoveCavern.cavernTag : CavernTag.Invalid;
                NavigationHandler.UpdateLatestCavernTag(newTag, nextTag);
            }
        }

        void OnCavernLeave(CavernHandler cavern)
        {
            GetCurrentMachineState().OnCavernLeave(cavern);
        }

        /// <summary>Calls when a player enters the cavern AI is in</summary>
        void OnPlayerEnterAICavern(CavernPlayerData data)
        {
            if (data.Handler == cavernManager.GetHandlerOfAILocation)
                GetCurrentMachineState().OnPlayerEnterAICavern(data);
        }

        void OnTunnelEnter(TunnelBehaviour tunnel)
        {
            navigationHandler.TunnelModeSteering();
        }

        public void OnTunnelLeave(TunnelBehaviour tunnel)
        {
            navigationHandler.CavernModeSteering();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (NavigationHandler != null) NavigationHandler.OnCollisionDetected().Invoke();
            if (HealthManager != null) HealthManager.OnCollisionDetected().Invoke();
        }

        //! Network events
        internal void Send_PlayAudio(bool is3D, AISound soundType)
        {
            if (neManager == null || !neManager.IsMasterClient) //! only master client can send this
                return;
            
            object[] content = new object[] { is3D, (int)soundType };
            neManager.RaiseEvent(ByteEvents.AI_PLAY_AUDIO, content, SendOptions.SendReliable);
        }

        private void Receive_PlayAudio(EventData eventData)
        {
            object[] content = (object[])eventData.CustomData;
            bool is3D = (bool)content[0];
            AISound soundType = (AISound)(int)content[1];

            if (is3D)
                AudioBank.Play3D(soundType, transform);
            else
                AudioBank.Play2D(soundType);
        }
		
		internal void Send_SetAnimation(AIAnim animType, float customLerpTime)
		{
			if (neManager == null || !neManager.IsMasterClient) //! only master client can send this
                return;
            
            object[] content = new object[] { (int)animType, customLerpTime };
            neManager.RaiseEvent(ByteEvents.AI_PLAY_ANIMATION, content, SendOptions.SendReliable);
		}
		
		private void Receive_SetAnimation(EventData eventData)
		{
			object[] content = (object[])eventData.CustomData;
			AIAnim animType = (AIAnim)(int)content[0];
			float customLerpTime = (float)content[1];
			
			AnimationManager.SetAnimation(animType, customLerpTime);
		}

        #endregion

        #region Transition Conditions

        Func<bool> IsAnticipating() => ()
            => RuntimeData.GetBrainState == BrainState.Anticipation && !isStunned;

        Func<bool> WantsToAmbush() => ()
            => RuntimeData.GetBrainState == BrainState.Ambush && !isStunned;

        Func<bool> WantsToHunt() => ()
            => RuntimeData.GetBrainState == BrainState.Hunt && !isStunned;

        Func<bool> CanJudge() => ()
            => RuntimeData.GetBrainState == BrainState.Judgement && !isStunned;

        Func<bool> IsRecovering() => ()
            => RuntimeData.GetBrainState == BrainState.Recovery && !isStunned;

        Func<bool> IsCooldown() => ()
            => RuntimeData.GetBrainState == BrainState.Cooldown && !isStunned;

        #endregion

        #region Control Methods

        /// <summary> Tries to set the AI to stunstate.
        /// Returns true AI can be stunned, false if AI is already stunned</summary>
        public bool TryToStun(float duration)
        {
            if (isStunned)
                return false;

            DetachAnyCarriedPlayer();

            stunDuration = duration;
            isStunned = true;
            if (onMasterClient)
            {
                NavigationHandler.SetAIStunned(isStunned);
                NavigationHandler.StunnedModeSteering();
            }
            Debug.LogWarning("I am stunned:" + isStunned);
            OnStunnedEvent?.Invoke(true);
            return true;
        }
        public void StopStun()
        {
            isStunned = false;
            if (onMasterClient)
            {
                NavigationHandler.SetAIStunned(isStunned);
                NavigationHandler.CavernModeSteering();
            }
            Debug.LogWarning("I am not stunned:" + isStunned);
            OnStunnedEvent?.Invoke(false);
        }

        [SerializeField] Collider leviathanCollider;
        public void ChangeColliderMaterial(PhysicMaterial physicMaterial)
        {
            leviathanCollider.material = physicMaterial;
        }

        public void RefreshPlayerReferences()
            => Players = FindObjectsOfType<PlayerController>().ToList();

        /// <summary>
        /// Attach or detach players.
        /// </summary>
        /// <param name="attachToMouth">Attach or detach</param>
        /// <remarks>Used for networking</remarks>
        public void AttachCarriedPlayerToMouth(bool attachToMouth)
        {
            if (MouthObject == null)
                MouthObject = FindObjectOfType<AIGraphicsHandler>().MouthObject;

            Transform mouth = MouthObject.transform;
            if (CarriedPlayer == null)
            {
                DetachAnyCarriedPlayer();
                return;
            }

            int grabbedPlayerID = CarriedPlayer.ViewID;
            if (attachToMouth)
            {
                CarriedPlayer.GetTarget.SetParent(mouth, true);
                CarriedPlayer.gameObject.layer = LayerMask.NameToLayer(RuntimeData.GrabbedPlayerLayer);
                CarriedPlayer.GetTarget.localPosition = Vector3.zero;

                //! Send event if host
                if (neManager.IsMasterClient)
                    neManager.RaiseEvent(ByteEvents.AI_GRAB_PLAYER, grabbedPlayerID);

                return;
            }

            //! Send event if host
            if (neManager.IsMasterClient)
                neManager.RaiseEvent(ByteEvents.AI_RELEASE_PLAYER, null);

            DetachAnyCarriedPlayer();
        }

        /// <summary>
        /// Detaches any carried player.
        /// </summary>
        /// <remarks>Used as local event</remarks>
        public void DetachAnyCarriedPlayer()
        {
            if (MouthObject == null)
                MouthObject = FindObjectOfType<AIGraphicsHandler>().MouthObject;

            //! Make sure any player in mouth is released
            PlayerController[] controllers = MouthObject.GetComponentsInChildren<PlayerController>();
            int freeLayerIndex = LayerMask.NameToLayer(RuntimeData.FreePlayerLayer);
            foreach (var player in controllers)
            {
                player.gameObject.layer = freeLayerIndex;
                player.SetIsCarried(false);
                player.SetIsTaggedByLeviathan(false);
            }

            if (CarriedPlayer != null)
            {
                CarriedPlayer.gameObject.layer = freeLayerIndex;
                CarriedPlayer.SetIsCarried(false);
                CarriedPlayer.SetIsTaggedByLeviathan(false);
            }
            CarriedPlayer = null;
            MouthObject.transform.DetachChildren();
        }

        /// <summary> Makes the AI carry its current target player. Safe for network call. </summary>
        public bool TryCarryTargetPlayer()
        {
            DetachAnyCarriedPlayer();
            if (CurrentTarget == null)
                return false;

            CurrentTarget.SetIsCarried(true);
            CarriedPlayer = CurrentTarget;
            AttachCarriedPlayerToMouth(true);
            return true;
        }

        /// <summary> Makes the AI drop any player. Safe for network call. </summary>
        public bool TryDropCarriedPlayer()
        {
            if (CarriedPlayer == null)
                return false;

            AttachCarriedPlayerToMouth(false);
            return true;
        }

        public void SpawnExplosivePointAt(Vector3 position)
        {
            ExplosivePoint.ExplosionSettings expSettings = new ExplosivePoint.ExplosionSettings();
            expSettings.Position = position;
            expSettings.Radius = SenseDetection.GetCurrentSenseDetectionRadius();
            expSettings.Force = -10.0f;
            expSettings.IgnoreLayers = MachineData.Engagement.JG_KnockbackIgnoreMasks;

            ExplosivePoint.Create(expSettings);
        }

        private void HandleEggDestroyedEvent(bool isDestroyed)
        {
            if (isDestroyed)
            {
                RuntimeData.SetIsEggDestroyed(true);
                RuntimeData.UpdateBonusConfidence(MachineData.EggDestroyedPermanentConfidence);
                return;
            }

            RuntimeData.SetIsEggDestroyed(false);
            RuntimeData.UpdateBonusConfidence(0);
        }

        public void TryToTargetClosestPlayerInAICavern() => TrySetCurrentTarget(GetClosestPlayerInAICavern());

        #endregion

        #region Data
        public void UpdateTargetMoveCavern(CavernHandler newCavern)
        {
            targetMoveCavern = newCavern;
            if (DebugEnabled) print("New target cavern: " + newCavern.cavernTag);
        }

        public void UpdateNextMoveCavern(CavernHandler newCavern)
        {
            nextMoveCavern = newCavern;
            if (DebugEnabled) print("Moving to next cavern: " + newCavern.cavernTag);
        }

        public void UpdateCachedCurrentCavern(CavernHandler newCavern)
        {
            if (newCavern == null || newCavern.cavernTag == CavernTag.Invalid)
                return;

            cachedCurrentCavern = newCavern;
            if (DebugEnabled) print("Updated current cavern cache to: " + newCavern.cavernTag);
        }

        public CavernHandler TargetMoveCavern => targetMoveCavern;
        public CavernHandler NextMoveCavern => nextMoveCavern;
        public CavernHandler CachedCurrentCavern => cachedCurrentCavern;
        #endregion

        #region Accesors

        public bool IsStunned => isStunned;
        public BrainState GetState => runtimeData.GetBrainState;

        public bool CheckForJudgementStateCondition()
        {
            if (CurrentTarget != null)
            {
                RuntimeData.SetBrainState(BrainState.Judgement);
                if (NavigationHandler.Data_IsOnQueuePath)
                    NavigationHandler.StopQueuedPath();

                if (DebugEnabled) "Spotted and entered engagement!".Msg();
                return true;
            }
            return false;
        }

        public PlayerController GetClosestPlayerInAICavern()
        {
            var handler = CavernManager.GetHandlerOfAILocation;
            if (handler != null)
                return handler.GetClosestPlayerTo(transform);
            
            return null;
        }

        public AIStateBase GetCurrentMachineState()
        {
            foreach (AIStateBase state in allStates) if (state.IsCurrentState) return state;

            Debug.LogError("No active state found!");
            return null;
        }
        public AIStateBase GetMachineState(BrainState state)
            => state switch
            {
                BrainState.Anticipation => anticipationState,
                BrainState.Hunt => huntState,
                BrainState.Recovery => recoveryState,
                BrainState.Cooldown => cooldownState,
                _ => ReturnDefaultAIState()
            };
        private AIStateBase ReturnDefaultAIState()
        {
            Debug.LogWarning("State not found!");
            return null;
        }

        public bool CanUpdate => (PhotonNetwork.IsMasterClient || isOffline) && (_playersAreReady || isOffline);
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;

        #endregion

        #region Interface Methods

        public bool IsLeviathan => true;
        public void TryToMakeRunAway()
        {
            RuntimeData.SetBrainState(BrainState.Recovery);
        }
        public GameObject Obj => gameObject;

        #endregion

        #region Verbose Shorthands

        public bool IsCarryingAPlayer(bool carriedMustBeTargetPlayer = false)
        {
            if (carriedMustBeTargetPlayer)
                return CarriedPlayer != null && CarriedPlayer == CurrentTarget;

            return CarriedPlayer != null;
        }

        #endregion

        #region Debugging
        private bool suspendStateLogic = false;

        private BrainState overrideState = BrainState.None;
        private bool startWithOverrideState = false;

        public void EnableBrain() => isEnabled = true;
        public void DisableBrain() => isEnabled = false;
        public bool StateSuspension => suspendStateLogic;
        public void SuspendState() => suspendStateLogic = true;
        public void ResumeState() => suspendStateLogic = false;
        public void SetOverrideState(BrainState state) => overrideState = state;
        public void StartWithOverrideState() => startWithOverrideState = true;

        #endregion
    }

    public enum AIDamageType
    {
        Thresh,
        Tail
    }
}
