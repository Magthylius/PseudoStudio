using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class HasJudgementThresholdExceededNode : BTNode
    {
        private AIBrain _brain;
        private int _thresholdIndex;


        public HasJudgementThresholdExceededNode(AIBrain brain, int thresholdIndex)
        {
            _brain = brain;
            _thresholdIndex = thresholdIndex;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if (_brain.RuntimeData.HasJudgementTimerOfIndexExceeded(_thresholdIndex))
            {
                Debug.LogWarning(("waht the fuck"));
                return NodeState.SUCCESS;
            }
            else
                return NodeState.FAILURE;
        }
    }
}
