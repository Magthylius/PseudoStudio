using System.Collections;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class TailWhipNode : BTNode
    {
        private AIBrain _brain;
        private AITailManager _tailManager;
        private bool _tailWhipDone;
        private bool _triggerOnce;
        private float _whipTimer;
        private float _whipTime;

        public TailWhipNode(AIBrain brain)
        {
            _brain = brain;
            _tailManager = brain.TailManager;
            _tailWhipDone = false;
            _triggerOnce = false;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if (!_triggerOnce)
            {
                _triggerOnce = true;
                _tailManager.EnableWhipStance();
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
                    _tailManager.DisableWhipStance();
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
