using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Tenshi.AIDolls;
using Hadal.AI.States;
using Hadal.Utility;
using NaughtyAttributes;
using Hadal.AI.GeneratorGrid;
using Tenshi.UnitySoku;

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

        [Header("Idle Setting")]
        IState idleState;
        [Foldout("Idle")] public float destinationChangeTimer;

        [Header("AI Engagement Setting")]
        IState engagementState;
        [Tooltip("Detection Radius"), Foldout("Aggressive")] public float detectionRadius;
        [Tooltip("Wall Detection Radius"), Foldout("Aggressive")] public float wallDetectionRadius;
        [Tooltip("Speed of Pin to Wall"), Foldout("Aggressive")] public int pinSpeed;

        [Foldout("Aggressive"), SerializeField] public LayerMask playerMask;
        [Foldout("Aggressive"), SerializeField] public LayerMask obstacleMask;

        [Header("Stunned Setting")]
        IState stunnedState;
        bool isStunned;
        [Foldout("Stun"), SerializeField] public float stunDuration;

        private void Awake()
        {
            GridGenerator.GridLoadedEvent += InitialiseStates;
            isGridInitialised = false;
            if (playerMask == default) playerMask = LayerMask.GetMask("Player", "LocalPlayer");
            if (obstacleMask == default) obstacleMask = LayerMask.GetMask("Obstacle");
            isStunned = false;
            InitialiseDebugStateSwitchTimer();
        }

        private void Update()
        {
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
            stateMachine.AddSequentialTransition(from: idleState, to: engagementState, withCondition: idleState.ShouldTerminate());
            stateMachine.AddSequentialTransition(from: engagementState, to: idleState, withCondition: engagementState.ShouldTerminate());

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

        private void InitialiseDebugStateSwitchTimer()
        {
            beIdle = true;
            switchTimer = this.Create_A_Timer().WithDuration(timerToSwitchState)
                                               .WithShouldPersist(true)
                                               .WithOnCompleteEvent(() => beIdle = !beIdle)
                                               .WithLoop(true);
            this.AttachTimer(switchTimer);
        }
        
        #endregion

        #region Control Methods
        /// <summary> Set the AI to stunstate</summary>
        /// <param name="statement">true if AI should be stun, false if AI shouldn't be stun</param>
        public void SetIsStunned(bool statement) => isStunned = statement;
        
        #endregion

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.1f);
            Gizmos.DrawSphere(transform.position, detectionRadius);
            Gizmos.color = new Color(1, 0, 1, 0.1f);
            Gizmos.DrawSphere(transform.position, wallDetectionRadius);
        }
    }
}
