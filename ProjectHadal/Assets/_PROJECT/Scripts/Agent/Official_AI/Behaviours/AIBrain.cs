using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Tenshi.AIDolls;
using Hadal.AI.States;
using NaughtyAttributes;

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        StateMachine stateMachine;
        public List<Transform> destinations;
        public List<Transform> playerTransforms;

        [Header("Idle Setting")]
        IState idle;
        public float destinationChangeTimer;

        [Header("AI Engagement Setting")]
        IState engagement;
        [Tooltip("Detection Radius"), Foldout("Aggressive")] public float detectionRadius;
        [Tooltip("Wall Detection Radius"), Foldout("Aggressive")] public float wallDetectionRadius;
        [Tooltip("Speed of Pin to Wall"), Foldout("Aggressive")] public int pinSpeed;

        [Foldout("Aggressive")] [SerializeField] public LayerMask playerMask;
        [Foldout("Aggressive")] [SerializeField] public LayerMask obstacleMask;

        private void Awake()
        {
            InitialiseStates();
            playerMask = LayerMask.GetMask("Player");
            obstacleMask = LayerMask.GetMask("Obstacle");
        }

        private void Start()
        {
            //! set default state
            stateMachine.SetState(idle);
        }

        private void Update()
        {
            stateMachine.MachineTick();
        }

        private void InitialiseStates()
        {
            //! instantiate classes
            stateMachine = new StateMachine();
            idle = new IdleState(this, destinationChangeTimer);
            engagement = new EngagementState(this);

            //! setup custom transitions
            stateMachine.AddSequentialTransition(from: idle, to: engagement, withCondition: EngagePlayer());
            stateMachine.AddSequentialTransition(from: engagement, to: idle, withCondition: LostTarget());
        }

        //! transition conditions (insert our design conditions here)
        Func<bool> EngagePlayer() => () =>
        {
            return Input.GetMouseButtonDown(0);
        };
        Func<bool> LostTarget() => () =>
        {
            return Input.GetMouseButtonDown(1);
        };

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.1f);
            Gizmos.DrawSphere(transform.position, detectionRadius);
            Gizmos.color = new Color(1, 0, 1, 0.1f);
            Gizmos.DrawSphere(transform.position, wallDetectionRadius);
        }
    }
}
