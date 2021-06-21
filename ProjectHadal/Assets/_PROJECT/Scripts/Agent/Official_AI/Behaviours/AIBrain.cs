using System;
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
using System.Collections;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Button = NaughtyAttributes.ButtonAttribute;

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        [ReadOnly, SerializeField] private bool enabled = true;

        [Header("Read-only data")]
        [ReadOnly, SerializeField] private CavernHandler targetMoveCavern;
        [ReadOnly, SerializeField] private CavernHandler nextMoveCavern;

        //[Header("Debugging")] 
        [Header("Module Components")]
        [SerializeField] private AIHealthManager healthManager;
        [SerializeField] private PointNavigationHandler navigationHandler;
        [SerializeField] private AISenseDetection senseDetection;
        [SerializeField] private AISightDetection sightDetection;
        [SerializeField] private AITailManager tailManager;
        [SerializeField] private AIDamageManager damageManager;
        [SerializeField] private AIGraphicsHandler graphicsHandler;
        [SerializeField] private CavernManager cavernManager;
        NetworkEventManager neManager;
        public AIHealthManager HealthManager => healthManager;
        public PointNavigationHandler NavigationHandler => navigationHandler;
        public AISenseDetection SenseDetection => senseDetection;
        public AISightDetection SightDetection => sightDetection;
        public AITailManager TailManager => tailManager;
        public AIDamageManager DamageManager => damageManager;
        public AIGraphicsHandler GraphicsHandler => graphicsHandler;
        public CavernManager CavernManager => cavernManager;

        private StateMachine stateMachine;
        private List<ILeviathanComponent> allAIComponents;
        private List<ILeviathanComponent> preUpdateComponents;
        private List<ILeviathanComponent> mainUpdateComponents;

        [Header("Runtime Data")]
        [SerializeField] private LeviathanRuntimeData runtimeData;
        [ReadOnly] public GameObject MouthObject;
        [ReadOnly] public List<PlayerController> Players;
        [ReadOnly] public PlayerController CurrentTarget;
        [ReadOnly] public PlayerController CarriedPlayer;

        [Header("Settings Data")]
        [SerializeField] private StateMachineData machineData;
        [SerializeField] private bool followNetworkManagerOfflineStatus;
        [SerializeField] private bool isOffline;
        public bool DebugEnabled;

        public LeviathanRuntimeData RuntimeData => runtimeData;
        public StateMachineData MachineData => machineData;
        private Rigidbody rBody;

        AIStateBase anticipationState;
        AIStateBase engagementState;
        AIStateBase recoveryState;
        AIStateBase cooldownState;

        AggressiveSubState eAggressiveState;
        AmbushSubState eAmbushState;
        JudgementSubState eJudgementState;

        List<AIStateBase> allStates;

        [Header("Stunned Settings (needs a relook)")]
        [SerializeField, ReadOnly] bool isStunned;
        public float stunDuration;
        AIStateBase stunnedState;

        private bool _playersAreReady;

        private void Awake()
        {
            if (!enabled) return;

            _playersAreReady = false;
            rBody = GetComponent<Rigidbody>();
            graphicsHandler = FindObjectOfType<AIGraphicsHandler>();
            isStunned = false;

            allAIComponents = GetComponentsInChildren<ILeviathanComponent>().ToList();
            preUpdateComponents = allAIComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.PreUpdate).ToList();
            mainUpdateComponents = allAIComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.MainUpdate).ToList();

            Players = new List<PlayerController>();
            CurrentTarget = null;
            CarriedPlayer = null;

            runtimeData.Awake_Initialise();
            navigationHandler.Initialise();
        }

        private void Start()
        {
            if (!enabled) return;

            neManager = NetworkEventManager.Instance;
            if (neManager != null && followNetworkManagerOfflineStatus)
                isOffline = neManager.isOfflineMode;

            if (DebugEnabled && isOffline)
                "Leviathan brain initialising in Offline mode.".Msg();

            allAIComponents.ForEach(i => i.Initialise(this));
            cavernManager = FindObjectOfType<CavernManager>();

            //! Event handling
            cavernManager.AIEnterCavernEvent += OnCavernEnter;
            cavernManager.PlayerEnterCavernEvent += OnPlayerEnterAICavern;
            cavernManager.AIEnterTunnelEvent += OnTunnelEnter;
            cavernManager.AILeftTunnelEvent += OnTunnelLeave;

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

            //neManager.AddListener(ByteEvents.AI_GRAB_EVENT, RE_AttachCarriedPlayerToMouth);
        }

        private void Update()
        {
            if (!CanUpdate || !enabled) return;
            float deltaTime = DeltaTime;
            preUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
            navigationHandler.DoUpdate(deltaTime);
            stateMachine?.MachineTick();
            mainUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
            HandleCarriedPlayer();
        }
        private void LateUpdate()
        {
            if (!CanUpdate || !enabled) return;
            stateMachine?.LateMachineTick();
            allAIComponents.ForEach(c => c.DoLateUpdate(DeltaTime));
        }
        private void FixedUpdate()
        {
            if (!CanUpdate || !enabled) return;
            float fixedDeltaTime = FixedDeltaTime;
            navigationHandler.DoFixedUpdate(fixedDeltaTime);
            stateMachine?.FixedMachineTick();
            allAIComponents.ForEach(c => c.DoFixedUpdate(fixedDeltaTime));
        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();

            //! Anticipation
            anticipationState = new AnticipationState(this);

            //! Engagement
            eAggressiveState = new AggressiveSubState();
            eAmbushState = new AmbushSubState();
            eJudgementState = new JudgementSubState();
            engagementState = new EngagementState(this, eAggressiveState, eAmbushState, eJudgementState);

            //! Recovery
            recoveryState = new RecoveryState(this);

            //! Cooldown
            cooldownState = new CooldownState(this);

            //! -setup custom transitions-
            stateMachine.AddEventTransition(to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddEventTransition(to: engagementState, withCondition: HasEngageObjective());
            stateMachine.AddEventTransition(to: recoveryState, withCondition: IsRecovering());
            stateMachine.AddEventTransition(to: cooldownState, withCondition: IsCooldown());

            allStates = new List<AIStateBase>
            {
                anticipationState,
                engagementState,
                recoveryState,
                cooldownState
            };
        }

        private void PlayersAreReadySignal()
        {
            PlayerManager.Instance.OnAllPlayersReadyEvent -= PlayersAreReadySignal;
            "Players are ready, Happy hunting!".Msg();
            _playersAreReady = true;
        }

        private readonly Vector3 vZero = Vector3.zero;
        private void HandleCarriedPlayer()
        {
            if (CarriedPlayer == null) return;
            CarriedPlayer.GetTarget.localPosition = vZero;
        }

        #region Event Handlers
        /// <summary>Calls when AI enters a cavern</summary>
        void OnCavernEnter(CavernHandler cavern)
        {
            GetCurrentMachineState().OnCavernEnter(cavern);
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
        #endregion

        #region Transition Conditions

        Func<bool> IsAnticipating() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Anticipation && !isStunned;
        };
        Func<bool> IsRecovering() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Recovery && !isStunned;
        };
        Func<bool> HasEngageObjective() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Engagement && !isStunned;
        };
        Func<bool> IsCooldown() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Cooldown && !isStunned;
        };

        public bool IsStunned => isStunned;

        #endregion

        #region Control Methods

        /// <summary> Tries to set the AI to stunstate.
        /// Returns true AI can be stunned, false if AI is already stunned</summary>
        public bool TryToStun(float duration)
        {
            if (isStunned)
                return false;

            stunDuration = duration;
            isStunned = true;
            NavigationHandler.SetAIStunned(isStunned);
            NavigationHandler.StunnedModeSteering();
            Debug.LogWarning("I am stunned:" + isStunned);
            return true;
        }
        public void StopStun()
        {
            isStunned = false;
            NavigationHandler.SetAIStunned(isStunned);
            NavigationHandler.CavernModeSteering();
            Debug.LogWarning("I am not stunned:" + isStunned);
        }

        public void ChangeColliderMaterial(PhysicMaterial physicMaterial)
        {
            gameObject.GetComponent<Collider>().material = physicMaterial;
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
            Transform mouth = MouthObject.transform;
            if (CarriedPlayer == null)
            {
                //Debug.LogError("null detach!");
                mouth.DetachChildren();
                return;
            }

            int grabbedPlayerID = CarriedPlayer.GetInfo.PhotonInfo.PView.ViewID;
            if (attachToMouth)
            {
                //Debug.LogWarning("Player grabbed");
                CarriedPlayer.GetTarget.SetParent(mouth, true);
                CarriedPlayer.gameObject.layer = LayerMask.NameToLayer(RuntimeData.GrabbedPlayerLayer);
                CarriedPlayer.GetTarget.localPosition = Vector3.zero;

                //! Send event if host
                if (neManager.IsMasterClient)
                {
                    neManager.RaiseEvent(ByteEvents.AI_GRAB_PLAYER, grabbedPlayerID);
                }

                return;
            }


            mouth.DetachChildren();

            //! Send event if host
            if (neManager.IsMasterClient)
            {
                neManager.RaiseEvent(ByteEvents.AI_RELEASE_PLAYER, null);
            }
            CarriedPlayer.gameObject.layer = LayerMask.NameToLayer(RuntimeData.FreePlayerLayer);
        }

        /// <summary>
        /// Detaches any carried player.
        /// </summary>
        /// <remarks>Used as local event</remarks>
        public void DetachAnyCarriedPlayer()
        {
            //! Make sure any player in mouth is released
            PlayerController[] controllers = MouthObject.GetComponentsInChildren<PlayerController>();
            foreach (var player in controllers)
                player.gameObject.layer = LayerMask.NameToLayer(RuntimeData.FreePlayerLayer);

            MouthObject.transform.DetachChildren();
            CarriedPlayer = null;
        }
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

        public CavernHandler TargetMoveCavern => targetMoveCavern;
        public CavernHandler NextMoveCavern => nextMoveCavern;
        #endregion

        #region Accesors

        public BrainState GetState => runtimeData.GetBrainState;
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
                BrainState.Engagement => engagementState,
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

        #region Debugging
        private bool suspendStateLogic = false;

        private BrainState overrideState = BrainState.None;
        private bool startWithOverrideState = false;

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
