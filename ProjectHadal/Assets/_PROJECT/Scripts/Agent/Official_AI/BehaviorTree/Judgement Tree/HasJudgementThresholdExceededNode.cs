namespace Hadal.AI.TreeNodes
{
    public class HasJudgementThresholdExceededNode : BTNode
    {
        private AIBrain _brain;
        private int _thresholdMultiplierType;

        public HasJudgementThresholdExceededNode(AIBrain brain, int thresholdMultiplierType)
        {
            _brain = brain;
            _thresholdMultiplierType = thresholdMultiplierType;
        }

        public override NodeState Evaluate()
        {
            if (_brain.GetJudgementTimerValue > _brain.GetJudgementThreshold(_thresholdMultiplierType))
                return NodeState.SUCCESS;
            return NodeState.FAILURE;
        }
    }
}
