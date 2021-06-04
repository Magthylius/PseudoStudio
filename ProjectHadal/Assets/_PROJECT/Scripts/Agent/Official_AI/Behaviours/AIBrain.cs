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
using Hadal.Player;

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        [Header("Module Components")]
        [SerializeField] private AIHealthManager healthManager;
        [SerializeField] private PointNavigationHandler navigationHandler;
        [SerializeField] private AISenseDetection senseDetection;
        [SerializeField] private AISightDetection sightDetection;
        [SerializeField] private CavernManager cavernManager;
        public AIHealthManager HealthManager => healthManager;
        public PointNavigationHandler NavigationHandler => navigationHandler;
        public AISenseDetection SenseDetection => senseDetection;
        public AISightDetection SightDetection => sightDetection;
        public CavernManager CavernManager => cavernManager;

        private StateMachine stateMachine;
        private List<ILeviathanComponent> allAIComponents;
        private List<ILeviathanComponent> preUpdateComponents;
        private List<ILeviathanComponent> mainUpdateComponents;

        [Header("Runtime Data")]
        [SerializeField] private LeviathanRuntimeData runtimeData;
        public GameObject MouthObject;
        [ReadOnly] public List<PlayerController> Players;
        [ReadOnly] public PlayerController CurrentTarget;
        [ReadOnly] public PlayerController CarriedPlayer;
        
        [Header("Settings Data")]
        [SerializeField] private StateMachineData machineData;
        [SerializeField] private bool isOffline;
		public bool DebugEnabled;

        public LeviathanRuntimeData RuntimeData => runtimeData;
        public StateMachineData MachineData => machineData;

        private Rigidbody rBody;
        IState idleState;
        IState anticipationState;
        IState recoveryState;
        IState engagementState;
        AggressiveSubState eAggressiveState;
        AmbushSubState eAmbushState;
        JudgementSubState eJudgementState;

        [Header("Stunned Settings (needs a relook)")]
        [SerializeField] public float stunDuration;
        IState stunnedState;
        bool isStunned;

        //! Callbacks for Agent2 assembly
        public Func<Transform, int> GetViewIDMethod;
        public Func<Transform, int, bool> ViewIDBelongsToTransMethod;
        public Action<Transform, bool> FreezePlayerMovementEvent;
        public void InvokeFreezePlayerMovementEvent(Transform player, bool shouldFreeze) => FreezePlayerMovementEvent?.Invoke(player, shouldFreeze);
        public Action<Transform, Vector3> ThreshPlayerEvent;
        public void InvokeForceSlamPlayerEvent(Transform player, Vector3 destination) => ThreshPlayerEvent?.Invoke(player, destination);

        //! Events
        public static event Action<Transform, AIDamageType> DamagePlayerEvent;
        internal void InvokeDamagePlayerEvent(Transform t, AIDamageType type) => DamagePlayerEvent?.Invoke(t, type);

        public bool CanUpdate => PhotonNetwork.IsMasterClient || isOffline;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody>();
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
            allAIComponents.ForEach(i => i.Initialise(this));
			cavernManager = FindObjectOfType<CavernManager>();
            
			//! State machine
			InitialiseStates();
            runtimeData.SetMainObjective(MainObjective.Anticipation);
			stateMachine.SetState(anticipationState);

			//! Runtime data
            RefreshPlayerReferences();
            runtimeData.Start_Initialise();
            runtimeData.UpdateCumulativeDamageThreshold(HealthManager.GetCurrentHealth);
        }

        private void Update()
        {
            if (!CanUpdate) return;
            float deltaTime = DeltaTime;
            preUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
            navigationHandler.DoUpdate(deltaTime);
            stateMachine?.MachineTick();
            mainUpdateComponents.ForEach(c => c.DoUpdate(deltaTime));
        }
        private void LateUpdate()
        {
            if (!CanUpdate) return;
            stateMachine?.LateMachineTick();
            allAIComponents.ForEach(c => c.DoLateUpdate(DeltaTime));
        }
        private void FixedUpdate()
        {
            if (!CanUpdate) return;
            float fixedDeltaTime = FixedDeltaTime;
            navigationHandler.DoFixedUpdate(fixedDeltaTime);
            stateMachine?.FixedMachineTick();
            allAIComponents.ForEach(c => c.DoFixedUpdate(fixedDeltaTime));
        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();

			// Anticipation
            anticipationState = new AnticipationState(this);

			// Engagement
            eAggressiveState = new AggressiveSubState();
            eAmbushState = new AmbushSubState();
            eJudgementState = new JudgementSubState();
            engagementState = new EngagementState(this, eAggressiveState, eAmbushState, eJudgementState);

			// Others
            recoveryState = new RecoveryState(this);
            stunnedState = new StunnedState(this);
            
            //! -setup custom transitions-
            stateMachine.AddEventTransition(to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddEventTransition(to: engagementState, withCondition: HasEngageObjective());
            stateMachine.AddEventTransition(to: recoveryState, withCondition: IsRecovering());

            //! Any state can go into stunnedState
            // stateMachine.AddEventTransition(to: stunnedState, withCondition: IsStunned());
            // stateMachine.AddSequentialTransition(from: stunnedState, to: idleState, withCondition: stunnedState.ShouldTerminate());
        }

        #region Transition Conditions

        Func<bool> IsAnticipating() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Anticipation;
        };
        Func<bool> IsRecovering() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Recover;
        };
        Func<bool> HasEngageObjective() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Engagement;
        };
		
        Func<bool> IsStunned() => () =>
        {
            return isStunned;
        };
        
        #endregion

        #region Control Methods
        /// <summary> Set the AI to stunstate</summary>
        /// <param name="statement">true if AI should be stun, false if AI shouldn't be stun</param>
        public void SetIsStunned(bool statement) => isStunned = statement;

        public void RefreshPlayerReferences()
            => Players = FindObjectsOfType<PlayerController>().ToList();

        #endregion

        /// <summary>Debug draw the radius</summary>
        void OnDrawGizmos()
        {
            // Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.1f);
            // Gizmos.DrawSphere(transform.position, detectionRadius);
            // Gizmos.color = new Color(1, 0, 1, 0.1f);
            // Gizmos.DrawSphere(transform.position, wallDetectionRadius);
        }

        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
    }

    public enum AIDamageType
    {
        Thresh,
        Tail
    }
}
