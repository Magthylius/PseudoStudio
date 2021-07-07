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
    public delegate void PhaseEvents(bool isStarting);
    
    public class AIBrain : MonoBehaviour, IAmLeviathan
    {
        [ReadOnly, SerializeField] private bool isEnabled = true;
        [ReadOnly, SerializeField] private bool onMasterClient;

        [Header("Read-only data")]
        [ReadOnly, SerializeField] private CavernHandler targetMoveCavern;
        [ReadOnly, SerializeField] private CavernHandler nextMoveCavern;

        [Header("Module Components")]
        [SerializeField] private AIHealthManager healthManager;
        [SerializeField] private PointNavigationHandler navigationHandler;
        [SerializeField] private AISenseDetection senseDetection;
        [SerializeField] private AISightDetection sightDetection;
        [SerializeField] private AITailManager tailManager;
        [SerializeField] private AIDamageManager damageManager;
        [SerializeField] private AIGameHandler gameHandler;
        [SerializeField] private AIGraphicsHandler graphicsHandler;
        [SerializeField] private CavernManager cavernManager;
        NetworkEventManager neManager;
        public AIHealthManager HealthManager => healthManager;
        public PointNavigationHandler NavigationHandler => navigationHandler;
        public AISenseDetection SenseDetection => senseDetection;
        public AISightDetection SightDetection => sightDetection;
        public AITailManager TailManager => tailManager;
        public AIDamageManager DamageManager => damageManager;
        public AIGameHandler GameHandler => gameHandler;
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

        private PlayerController carriedPlayer;

        //[ReadOnly]
        public PlayerController CarriedPlayer
        {
            get { return carriedPlayer; }
            set
            {
                carriedPlayer = value;
                Debug.LogWarning("Carried player changed into: " + value);
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

        AIStateBase idleState;
        AIStateBase anticipationState;
        AIStateBase engagementState;
        AIStateBase recoveryState;
        AIStateBase cooldownState;
        AIStateBase lureState;

        AmbushState ambushState;
        JudgementState judgementState;

        List<AIStateBase> allStates;

        [Header("Stunned Settings (needs a relook)")]
        [SerializeField, ReadOnly] bool isStunned;
        public float stunDuration;
        AIStateBase stunnedState;

        private bool _playersAreReady;

        private void Awake()
        {
            if (!isEnabled || isOffline) return;

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
            neManager = NetworkEventManager.Instance;
            if (neManager != null && followNetworkManagerOfflineStatus)
                isOffline = neManager.isOfflineMode;

            onMasterClient = PhotonNetwork.IsMasterClient || isOffline;
            if (!onMasterClient)
            {
                healthManager.Initialise(this);
                return;
            }
            if (!isEnabled) return;

            Setup();
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
            allAIComponents.ForEach(c => c.DoLateUpdate(DeltaTime));
        }
        private void FixedUpdate()
        {
            if (!onMasterClient) return;
            if (!CanUpdate || !isEnabled) return;
            float fixedDeltaTime = FixedDeltaTime;
            navigationHandler.DoFixedUpdate(fixedDeltaTime);
            stateMachine?.FixedMachineTick();
            allAIComponents.ForEach(c => c.DoFixedUpdate(fixedDeltaTime));
        }

        void Setup()
        {
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

        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();

            //! Idle
            idleState = new IdleState(this);

            //! Anticipation
            anticipationState = new AnticipationState(this);

            //! Engagement
            ambushState = new AmbushState(this);
            judgementState = new JudgementState(this);
            engagementState = new EngagementState(this);

            //! Recovery
            recoveryState = new RecoveryState(this);

            //! Cooldown
            cooldownState = new CooldownState(this);

            //! Lure
            lureState = new LureState(this);

            //! -setup custom transitions-
            stateMachine.AddEventTransition(to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddEventTransition(to: judgementState, withCondition: CanJudge());
            stateMachine.AddEventTransition(to: ambushState, withCondition: WantsToAmbush());
            stateMachine.AddEventTransition(to: engagementState, withCondition: HasEngageObjective());
            stateMachine.AddEventTransition(to: recoveryState, withCondition: IsRecovering());
            stateMachine.AddEventTransition(to: cooldownState, withCondition: IsCooldown());
            stateMachine.AddEventTransition(to: idleState, withCondition: IsIdle());
            stateMachine.AddEventTransition(to: lureState, withCondition: IsLure());

            allStates = new List<AIStateBase>
            {
                anticipationState,
                judgementState,
                ambushState,
                engagementState,
                recoveryState,
                cooldownState,
                idleState,
                lureState
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
        Func<bool> CanJudge() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Judgement && !isStunned;
        };
        Func<bool> WantsToAmbush() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Ambush && !isStunned;
        };
        Func<bool> HasEngageObjective() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Engagement && !isStunned && cavernManager.GetCavernTagOfAILocation() != CavernTag.Invalid;
        };
        Func<bool> IsCooldown() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Cooldown && !isStunned;
        };

        Func<bool> IsIdle() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Idle && !isStunned;
        };

        Func<bool> IsLure() => () =>
        {
            return RuntimeData.GetBrainState == BrainState.Lure && !isStunned && RuntimeData.GetBrainState != BrainState.Engagement;
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

            DetachAnyCarriedPlayer();

            stunDuration = duration;
            isStunned = true;
            NavigationHandler.Disable();
            NavigationHandler.SetAIStunned(isStunned);
            NavigationHandler.StunnedModeSteering();
            Debug.LogWarning("I am stunned:" + isStunned);
            return true;
        }
        public void StopStun()
        {
            isStunned = false;
            NavigationHandler.Enable();
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
            if (MouthObject == null)
            {
                MouthObject = FindObjectOfType<AIGraphicsHandler>().MouthObject;
            }
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
            if (MouthObject == null)
            {
                MouthObject = FindObjectOfType<AIGraphicsHandler>().MouthObject;
            }

            //! Make sure any player in mouth is released
            PlayerController[] controllers = MouthObject.GetComponentsInChildren<PlayerController>();
            foreach (var player in controllers)
            {
                player.gameObject.layer = LayerMask.NameToLayer(RuntimeData.FreePlayerLayer);
                player.SetIsCarried(false);
            }

            MouthObject.transform.DetachChildren();
            CarriedPlayer = null;
            NavigationHandler.StopCustomPath(true);
        }

        /// <summary>
        /// Makes the AI carry its current target player
        /// </summary>
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
