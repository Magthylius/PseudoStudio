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
        private bool _reportResult;
        private NodeState _state;

        public CarryTargetNode(AIBrain brain, float carrySucceedDistance, float carryWaitTime)
        {
            _brain = brain;
            _succeedDistance = carrySucceedDistance;
            _waitTime = carryWaitTime;
            _isWaiting = false;
            _reportResult = false;
            _state = NodeState.FAILURE;
        }

        public override NodeState Evaluate()
        {
            if (_isWaiting) return NodeState.RUNNING;
            if (_reportResult)
            {
                ResetNode();
                return _state;
            }

            _isWaiting = true;
            _brain.StartCoroutine(WaitTimerRoutine());
            return NodeState.RUNNING;
        }

        private IEnumerator WaitTimerRoutine()
        {
            while (_isWaiting == true)
            {
                if (TickWaitTimer(_brain.DeltaTime) <= 0f)
                {
                    ResetWaitTimer();
                    if (_brain.CurrentTarget != null)
                    {
                        CoroutineData data = new CoroutineData(_brain, TryCarryPlayer());
                        yield return data.Coroutine;
                        _state = (NodeState)data.Result;
                    }
                    else
                    {
                        _state = NodeState.FAILURE;
                    }
                    _isWaiting = false;
                    _reportResult = true;
                }
                yield return null;
            }
        }

        private IEnumerator TryCarryPlayer()
        {
            float distance = Vector3.Distance(_brain.transform.position, _brain.CurrentTarget.transform.position);
            if (distance <= _succeedDistance)
            {
                // Function to catch player in mouth
                _brain.CarriedPlayer = _brain.CurrentTarget;
                yield return NodeState.SUCCESS;
            }
            else
            {
                // Do nothing
                _brain.CarriedPlayer = null;
                yield return NodeState.FAILURE;
            }
        }

        private void ResetNode()
        {
            _isWaiting = false;
            _reportResult = false;
        }
        private float TickWaitTimer(in float deltaTime) => _waitTimer -= deltaTime;
        private void ResetWaitTimer() => _waitTimer = _waitTime;
    }
}