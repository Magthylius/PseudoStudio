using System;
using System.Collections;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class CarryTargetNode : BTNode
    {
        private AIBrain _brain;
        private float _succeedDistance;
        private float _waitTimer;
        private float _waitTime;
        private bool _isWaiting;

        public CarryTargetNode(AIBrain brain, float carrySucceedDistance, float carryWaitTime)
        {
            _brain = brain;
            _succeedDistance = carrySucceedDistance;
            _waitTime = carryWaitTime;
            _isWaiting = true;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if (_isWaiting)
            {
                if (TickWaitTimer(_brain.DeltaTime) <= 0f)
                {
                    ResetWaitTimer();
                    NodeState state = (_brain.CurrentTarget != null) ? TryCarryPlayer() : NodeState.FAILURE;
                    _isWaiting = false;
                    return state;
                }
            }
            /*else
            {
                _isWaiting = true;
                return NodeState.RUNNING;
            }*/

            return NodeState.RUNNING;
        }

        private NodeState TryCarryPlayer()
        {
            float distance = Vector3.Distance(_brain.transform.position, _brain.CurrentTarget.transform.position);
            if (distance <= _succeedDistance)
            {
                _brain.CurrentTarget.SetIsCarried(true);
                _brain.CarriedPlayer = _brain.CurrentTarget;
                _brain.AttachCarriedPlayerToMouth(true);
                return NodeState.SUCCESS;
            }

            _brain.CurrentTarget.SetIsCarried(false);
            _brain.CarriedPlayer = null;
            _brain.AttachCarriedPlayerToMouth(false);
            return NodeState.FAILURE;
        }

        private float TickWaitTimer(in float deltaTime) => _waitTimer -= deltaTime;
        private void ResetWaitTimer() => _waitTimer = _waitTime;
    }
}