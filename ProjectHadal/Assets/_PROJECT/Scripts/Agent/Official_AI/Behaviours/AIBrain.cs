using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Hadal.AI.States;
using Hadal.AI.Caverns;
using Photon.Pun;
using System.Linq;
using Hadal.Player;
using Tenshi;
using NaughtyAttributes;
using Tenshi.UnitySoku;

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

        [Header("Information")]
        [SerializeField] private List<Transform> playerTransforms;
        [SerializeField] private PlayerController targetingPlayer;
        [SerializeField] private PlayerController carriedPlayer;
        public List<Transform> PlayerTransforms => playerTransforms;
        public PlayerController TargetingPlayer { get => targetingPlayer; set => targetingPlayer = value; }
        public PlayerController CarriedPlayer { get => carriedPlayer; set => carriedPlayer = value; }

        internal Rigidbody rb;

        [Header("Confidence Settings")]
        [SerializeField] private bool randomiseOnStart;
        [SerializeField] private int minConfidence;
        [SerializeField] private int maxConfidence;
        [SerializeField] private int startingConfidence;
        [SerializeField, Tenshi.ReadOnly] private int confidence;
        [SerializeField, Tenshi.ReadOnly] private int bonusConfidence;
        public int ActualConfidenceValue => Mathf.Clamp(confidence + bonusConfidence, minConfidence, maxConfidence);
        public float NormalisedConfidence => ActualConfidenceValue.NormaliseValue(minConfidence, maxConfidence);
        public void UpdateConfidenceValue(int difference) => confidence = Mathf.Clamp(confidence + difference, minConfidence, maxConfidence);
        public void UpdateBonusConfidence(int difference) => bonusConfidence += difference;
        
        IState idleState;

        [Header("Target Settings")]
        public LayerMask playerMask;
        [SerializeField] private float targetChangeTimer;

        [Header("Anticipation Settings")]
        [SerializeField, Tenshi.ReadOnly] MainObjective objective;
        [SerializeField, Tenshi.ReadOnly] EngagementObjective engagementObjective;
        IState anticipationState;
        public MainObjective Objective => objective;
        public EngagementObjective EngagementObjective => engagementObjective;
        
        [Header("Engagement Settings")]
        public float playerDetectionRadius;
        public float playerDetectionAngle;
        IState engagementState;

        [Header("E. Aggressive Settings")]
        [SerializeField] public LayerMask obstacleMask;
        AggressiveSubState eAggressiveState;

        [Header("E. Ambush Settings")]
        [SerializeField] private float ambushTimoutTime;
        AmbushSubState eAmbushState;

        [Header("E. Judgement Settings")]
        [SerializeField] private float cummulativeDamageThreshold;
        private float cummulativeDamage;
        [SerializeField] private float judgeTickTime;
        JudgementSubState eJudgementState;
        [SerializeField] private float judgementThreshold;
        [SerializeField] private float jThreshold1Multiplier;
        [SerializeField] private float jThreshold2Multiplier;
        [SerializeField] private float jThreshold3Multiplier;
        [SerializeField] private float jThreshold4Multiplier;
        private float judgementStoptimer;
        public float GetJudgementTimerValue => judgementStoptimer;
        public float GetJudgementThreshold(int multiplierType)
        {
            return multiplierType switch
            {
                1 => judgementThreshold * jThreshold1Multiplier,
                2 => judgementThreshold * jThreshold2Multiplier,
                3 => judgementThreshold * jThreshold3Multiplier,
                4 => judgementThreshold * jThreshold4Multiplier,
                _ => judgementThreshold
            };
        }
        public void TickJudgementTimer(in float deltaTime) => judgementStoptimer += deltaTime;
        public void ResetJudgementTimer() => judgementStoptimer = 0.0f;
        public void ResetCummulativeDamage() => cummulativeDamage = 0f;
        public void AddCummulativeDamage(float damage) => cummulativeDamage += Mathf.Abs(damage);
        public bool CummulativeDamageExceeded() => cummulativeDamage > cummulativeDamageThreshold;

        [Header("Recovery Settings")]
        [SerializeField] private float recoveryTimoutTime;
        IState recoveryState;

        [Header("Stunned Settings")]
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

        private void Awake()
        {
            if (playerMask == default) playerMask = LayerMask.GetMask("LocalPlayer");
            if (obstacleMask == default) obstacleMask = LayerMask.GetMask("Wall");
            rb = GetComponent<Rigidbody>();
            isStunned = false;
            objective = MainObjective.None;
            if (randomiseOnStart) confidence = UnityEngine.Random.Range(minConfidence, maxConfidence + 1);
            else confidence = startingConfidence;
            
            allAIComponents = GetComponentsInChildren<ILeviathanComponent>().ToList();
            preUpdateComponents = allAIComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.PreUpdate).ToList();
            mainUpdateComponents = allAIComponents.Where(c => c.LeviathanUpdateMode == UpdateMode.MainUpdate).ToList();
        }

        private void Start()
        {
            allAIComponents.ForEach(i => i.Initialise(this));
            cavernManager = FindObjectOfType<CavernManager>();
            InitialiseStates();
            stateMachine.SetState(idleState);
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            preUpdateComponents.ForEach(c => c.DoUpdate(DeltaTime));
            stateMachine?.MachineTick();
            mainUpdateComponents.ForEach(c => c.DoUpdate(DeltaTime));
        }
        private void LateUpdate()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            stateMachine?.LateMachineTick();
            allAIComponents.ForEach(c => c.DoLateUpdate(DeltaTime));
        }
        private void FixedUpdate()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            stateMachine?.FixedMachineTick();
            allAIComponents.ForEach(c => c.DoFixedUpdate(FixedDeltaTime));
        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();

            idleState = new IdleState(this);
            anticipationState = new AnticipationState(this);

            eAggressiveState = new AggressiveSubState();
            eAmbushState = new AmbushSubState();
            eJudgementState = new JudgementSubState();
            engagementState = new EngagementState(this, eAggressiveState, eAmbushState, eJudgementState);

            recoveryState = new RecoveryState(this);
            stunnedState = new StunnedState(this);
            
            //! -setup custom transitions-
            stateMachine.AddSequentialTransition(from: idleState, to: anticipationState, withCondition: IsAnticipating());
            stateMachine.AddSequentialTransition(from: anticipationState, to: engagementState, withCondition: HasEngageObjective());
            stateMachine.AddSequentialTransition(from: engagementState, to: recoveryState, withCondition: IsRecovering());

            stateMachine.AddEventTransition(to: idleState, withCondition: ResetStates());

            //! Any state can go into stunnedState
            // stateMachine.AddEventTransition(to: stunnedState, withCondition: IsStunned());
            // stateMachine.AddSequentialTransition(from: stunnedState, to: idleState, withCondition: stunnedState.ShouldTerminate());
        }

        public void InjectPlayerTransforms(List<Transform> players) => playerTransforms = players;

        #region Transition Conditions

        // TODO: POC timer(need to optimize)
        // [SerializeField] float timerToSwitchState = 40.0f;
        // Timer switchTimer;
        // bool beIdle;
        Func<bool> IsAnticipating() => () =>
        {
            return false;
        };
        Func<bool> IsRecovering() => () =>
        {
            return false;
        };
        Func<bool> HasEngageObjective() => () =>
        {
            return objective != MainObjective.None;
        };
        Func<bool> ResetStates() => () =>
        {
            return false;
        };
        Func<bool> IsStunned() => () =>
        {
            return isStunned;
        };

        //TODO: Do the actual condition for switching states
        /// <summary>Switch to different state when time's up</summary>
        private void InitialiseDebugStateSwitchTimer()
        {
            // beIdle = true;
            // switchTimer = this.Create_A_Timer().WithDuration(timerToSwitchState)
            //                                    .WithShouldPersist(true)
            //                                    .WithOnCompleteEvent(() =>
            //                                    {
            //                                        beIdle = !beIdle;
            //                                        if (beIdle)
            //                                        {
            //                                            "I should be idle".Msg();
            //                                        }
            //                                        else
            //                                        {
            //                                            "I should be engage".Msg();
            //                                        }
            //                                    })
            //                                    .WithLoop(true);
            //    .WithOnUpdateEvent(_ =>
            //    {
            //        $"Switch state timer: {(100f * switchTimer.GetCompletionRatio):F2}%".Msg();
            //    });
            // this.AttachTimer(switchTimer);
        }

        #endregion

        #region Control Methods
        /// <summary> Set the AI to stunstate</summary>
        /// <param name="statement">true if AI should be stun, false if AI shouldn't be stun</param>
        public void SetIsStunned(bool statement) => isStunned = statement;

        public void SetObjective(MainObjective objective) => this.objective = objective;

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
