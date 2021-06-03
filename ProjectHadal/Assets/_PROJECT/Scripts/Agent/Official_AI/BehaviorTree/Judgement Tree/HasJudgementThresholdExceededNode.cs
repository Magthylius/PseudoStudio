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

        public override NodeState Evaluate()
        {
            if (_brain.RuntimeData.GetJudgementTimerValue > _brain.MachineData.Engagement.GetJudgementTimerThreshold(_thresholdIndex))
                return NodeState.SUCCESS;
            return NodeState.FAILURE;
        }
    }
}
