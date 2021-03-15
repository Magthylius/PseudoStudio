using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tenshi.AIDolls;
using Hadal.AI.States;
using Hadal.Utility;
using NaughtyAttributes;
using Hadal.AI.GeneratorGrid;
using Tenshi.UnitySoku;
using Tenshi;
using Photon.Pun;

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] AIHealthManager healthManager;
        public AIHealthManager HealthManager => healthManager;
        StateMachine stateMachine;
        public List<Transform> destinations;
        public List<Transform> playerTransforms;
        bool isGridInitialised = false;
        internal Rigidbody rb;

        [Header("Idle Setting")]
        IState idleState;
        [Foldout("Idle")] public float destinationChangeTimer;
        [Foldout("Idle"), SerializeField] internal float idleSpeed;

        [Header("AI Engagement Setting")]
        IState engagementState;
        [Tooltip("Detection Radius"), Foldout("Aggressive")] public float detectionRadius;
        [Tooltip("Wall Detection Radius"), Foldout("Aggressive")] public float wallDetectionRadius;
        [Tooltip("Speed of Pin to Wall"), Foldout("Aggressive")] public int pinSpeed;

        [Header("AI Aggressive Setting")]
        [Foldout("Aggressive"), SerializeField] public LayerMask playerMask;
        [Foldout("Aggressive"), SerializeField] public LayerMask obstacleMask;
        public Func<Transform, int> GetViewIDMethod;
        public Func<Transform, int, bool> ViewIDBelongsToTransMethod;
        public Action<Transform, bool> FreezePlayerMovementEvent;
        public void InvokeFreezePlayerMovementEvent(Transform player, bool shouldFreeze) => FreezePlayerMovementEvent?.Invoke(player, shouldFreeze);
        public Action<Transform, Vector3> ForceSlamPlayerEvent;
        public void InvokeForceSlamPlayerEvent(Transform player, Vector3 destination) => ForceSlamPlayerEvent?.Invoke(player, destination);

        [Header("Stunned Setting")]
        IState stunnedState;
        bool isStunned;
        [Foldout("Stun"), SerializeField] public float stunDuration;

        //! Events
        public static event Action<Transform, AIDamageType> DamagePlayerEvent;
        internal void InvokeDamagePlayerEvent(Transform t, AIDamageType type) => DamagePlayerEvent?.Invoke(t, type);

        private void Awake()
        {
            GridGenerator.GridLoadedEvent += InitialiseStates;
            isGridInitialised = false;
            if (playerMask == default) playerMask = LayerMask.GetMask("LocalPlayer");
            if (obstacleMask == default) obstacleMask = LayerMask.GetMask("Wall");
            rb = this.GetComponent<Rigidbody>();
            isStunned = false;
            InitialiseDebugStateSwitchTimer();
        }

        private void Start()
        {
            destinations = new List<Transform>();
            destinations = AIManager.Instance.GetPositions().ToList();
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            HandlePseudoStart();
            stateMachine?.MachineTick();
        }

        private void InitialiseStates(Grid grid)
        {
            GridGenerator.GridLoadedEvent -= InitialiseStates;

            //! instantiate classes
            stateMachine = new StateMachine();
            idleState = new IdleState(this, destinationChangeTimer);
            engagementState = new EngagementState(this);
            stunnedState = new StunnedState(this);

            //! -setup custom transitions-
            //! Idle to Engagement and vice versa
            stateMachine.AddSequentialTransition(from: idleState, to: engagementState, withCondition: BeEngage());
            stateMachine.AddSequentialTransition(from: engagementState, to: idleState, withCondition: BeIdle());

            //! Any state can go into stunnedState
            stateMachine.AddEventTransition(to: stunnedState, withCondition: IsStunned());

            //! StunState return to idleState
            stateMachine.AddSequentialTransition(from: stunnedState, to: idleState, withCondition: stunnedState.ShouldTerminate());
            
            isGridInitialised = true;
        }

        private void HandlePseudoStart()
        {
            if (!isGridInitialised) return;

            isGridInitialised = false;
            //! set default state
            stateMachine.SetState(idleState);
        }

        public void InjectPlayerTransforms(List<Transform> players)
        {
            playerTransforms = players;
        }

        #region Transition Conditions

        // TODO: POC timer(need to optimize)
        [SerializeField] float timerToSwitchState = 40.0f;
        Timer switchTimer;
        bool beIdle;
        Func<bool> BeEngage() => () =>
        {
            return !beIdle;
        };
        Func<bool> BeIdle() => () =>
        {
            return beIdle;
        };
        Func<bool> IsStunned() => () =>
        {
            return isStunned;
        };

        //TODO: Do the actual condition for switching states
        /// <summary>Switch to different state when time's up</summary>
        private void InitialiseDebugStateSwitchTimer()
        {
            beIdle = true;
            switchTimer = this.Create_A_Timer().WithDuration(timerToSwitchState)
                                               .WithShouldPersist(true)
                                               .WithOnCompleteEvent(() =>
                                               {
                                                   beIdle = !beIdle;
                                                   if (beIdle)
                                                   {
                                                       "I should be idle".Msg();
                                                   }
                                                   else
                                                   {
                                                       "I should be engage".Msg();
                                                   }
                                               })
                                               .WithLoop(true);
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

        #endregion

        /// <summary>Debug draw the radius</summary>
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.1f);
            Gizmos.DrawSphere(transform.position, detectionRadius);
            Gizmos.color = new Color(1, 0, 1, 0.1f);
            Gizmos.DrawSphere(transform.position, wallDetectionRadius);
        }
    }

    public enum AIDamageType
    {
        Pin,
        Tail
    }
}
