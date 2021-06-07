using System.Collections;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class TailWhipNode : BTNode
    {
        private AIBrain _brain;
        private Collider _tailCollider;
        private bool _tailWhipDone;
        private bool _triggerOnce;
        private float _whipTimer;
        private float _whipTime;

        public TailWhipNode(AIBrain brain)
        {
            _brain = brain;
            _tailWhipDone = false;
            _triggerOnce = false;
        }

        public override NodeState Evaluate()
        {
            if (!_triggerOnce)
            {
                _triggerOnce = true;
                // trigger tailwhip code here
                _brain.StartCoroutine(WhipRoutine());
            }

            if (!_tailWhipDone)
                return NodeState.RUNNING;

            "Whips something?".Msg();
            return NodeState.SUCCESS;
        }

        private IEnumerator WhipRoutine()
        {
            while (!_tailWhipDone)
            {
                if (TickWhipTimer(_brain.DeltaTime) <= 0)
                {
                    ResetWhipTimer();
                    // disable collider here
                    _tailWhipDone = true;
                }
                yield return null;
            }
            _triggerOnce = false;
        }

        private float TickWhipTimer(in float deltaTime) => _whipTimer -= deltaTime;
        private void ResetWhipTimer() => _whipTimer = _whipTime;
    }
}
