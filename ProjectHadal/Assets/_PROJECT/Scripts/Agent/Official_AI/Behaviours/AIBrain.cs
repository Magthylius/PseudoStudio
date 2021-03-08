using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Tenshi.AIDolls;

namespace Hadal.AI
{
    public class AIBrain : MonoBehaviour
    {
        StateMachine stateMachine;
        public List<Transform> destinations;

        [Header("Idle Setting")]
        IState idle;
        public float destinationChangeTimer;

        [Header("Engagement Setting")]
        IState engagement;

        private void Awake()
        {
            InitialiseStates();
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
            engagement = new EngagementState();

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
    }
}
