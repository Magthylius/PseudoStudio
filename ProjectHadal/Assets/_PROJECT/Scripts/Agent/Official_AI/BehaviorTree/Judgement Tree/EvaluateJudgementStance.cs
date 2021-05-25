using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class EvaluateJudgementStanceNode : BTNode
    {
        private AIBrain _brain;
        private float _confidenceThreshold;
        public EvaluateJudgementStanceNode(AIBrain brain, float confidenceThreshold)
        {
            _brain = brain;
            _confidenceThreshold = confidenceThreshold;
        }

        public override NodeState Evaluate()
        {
            bool isAggressive = _brain.Confidence < _confidenceThreshold;
            if (isAggressive)
            {
                return NodeState.SUCCESS;
            }
            return NodeState.FAILURE;
        }
    }
}
