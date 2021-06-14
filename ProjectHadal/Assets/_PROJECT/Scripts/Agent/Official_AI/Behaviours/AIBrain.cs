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

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        [Header("Read-only data")]
        [ReadOnly, SerializeField]
        private CavernHandler targetMoveCavern;

        [Header("Module Components")]
        [SerializeField] private AIHealthManager healthManager;
        [SerializeField] private PointNavigationHandler navigationHandler;
        [SerializeField] private AISenseDetection senseDetection;
        [SerializeField] private AISightDetection sightDetection;
        [SerializeField] private AITailManager tailManager;
        [SerializeField] private AIDamageManager damageManager;
        [SerializeField] private CavernManager cavernManager;
        public AIHealthManager HealthManager => healthManager;
        public PointNavigationHandler NavigationHandler => navigationHandler;
        public AISenseDetection SenseDetection => senseDetection;
        public AISightDetection SightDetection => sightDetection;
        public AITailManager TailManager => tailManager;
        public AIDamageManager DamageManager => damageManager;
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

        AIStateBase anticipationState;
        AIStateBase engagementState;
        AIStateBase recoveryState;
        AIStateBase cooldownState;

        AggressiveSubState eAggressiveState;
        AmbushSubState eAmbushState;
        JudgementSubState eJudgementState;

        List<AIStateBase> allStates;

        [Header("Stunned Settings (needs a relook)")]
        [SerializeField] public float stunDuration;
        AIStateBase stunnedState;
        bool isStunned;

        private void Awake()
        {
			if (DebugEnabled && isOffline)
				"Leviathan brain initialising in Offline mode.".Msg();
			
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
            cavernManager.AIEnterCavernEvent += OnCavernEnter;
            cavernManager.PlayerEnterCavernEvent += OnPlayerEnterAICavern;

            //! State machine
            InitialiseStates();
            runtimeData.SetMainObjective(MainObjective.Anticipation);
            stateMachine.SetState(anticipationState);

            //! Runtime data
            RefreshPlayerReferences();
            runtimeData.Start_Initialise();
            navigationHandler.SetCavernManager(cavernManager);
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

            stunnedState = new StunnedState(this);

            //! -setup custom transitions-
            stateMachine.AddEventTransition(to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddEventTransition(to: engagementState, withCondition: HasEngageObjective());
            stateMachine.AddEventTransition(to: recoveryState, withCondition: IsRecovering());

            //! Any state can go into stunnedState
            stateMachine.AddEventTransition(to: stunnedState, withCondition: IsStunned());

            allStates = new List<AIStateBase> { anticipationState, engagementState, recoveryState, cooldownState };
        }

        #region Event Handlers
        /// <summary>Calls when AI enters a cavern</summary>
        public void OnCavernEnter(CavernHandler cavern)
        {
            //stateMachine.CurrentState.OnCavernEnter();
            GetCurrentState().OnCavernEnter(cavern);
        }

        /// <summary>Calls when a player enters the cavern AI is in</summary>
        public void OnPlayerEnterAICavern(CavernPlayerData data)
        {
            if (data.Handler == cavernManager.GetHandlerOfAILocation)
                GetCurrentState().OnPlayerEnterAICavern(data);
        }
        #endregion

        #region Transition Conditions

        Func<bool> IsAnticipating() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Anticipation && !isStunned;
        };
        Func<bool> IsRecovering() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Recover && !isStunned;
        };
        Func<bool> HasEngageObjective() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Engagement && !isStunned;
        };
        Func<bool> IsCooldown() => () =>
        {
            return RuntimeData.GetMainObjective == MainObjective.Cooldown && !isStunned;
        };

        Func<bool> IsStunned() => () =>
        {
            return isStunned;
        };

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
            return true;
        }
        public void StopStun() => isStunned = false;

        public void RefreshPlayerReferences()
            => Players = FindObjectsOfType<PlayerController>().ToList();

        public void AttachCarriedPlayerToMouth(bool attachToMouth)
        {
            if (CarriedPlayer == null)
            {
                MouthObject.transform.DetachChildren();
                return;
            }

            if (attachToMouth)
            {
                CarriedPlayer.GetTarget.SetParent(MouthObject.transform);
                return;
            }

            MouthObject.transform.DetachChildren();
        }

        #endregion

        #region Data
        public void UpdateTargetMoveCavern(CavernHandler newCavern)
        {
            targetMoveCavern = newCavern;
            //NavPoint[] nextCavernPoints = CavernManager.GetHandlerOfAILocation.GetEntryNavPoints(newCavern);
            //NavigationHandler.SetQueuedPath(nextCavernPoints);
        }

        public CavernHandler TargetMoveCavern => targetMoveCavern;
        #endregion

        public AIStateBase GetCurrentState()
        {
            foreach (AIStateBase state in allStates) if (state.IsCurrentState) return state;

            Debug.LogError("No active state found!");
            return null;
        }
        public bool CanUpdate => PhotonNetwork.IsMasterClient || isOffline;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
    }

    public enum AIDamageType
    {
        Thresh,
        Tail
    }
}
