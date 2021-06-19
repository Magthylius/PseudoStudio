namespace Hadal.AI.TreeNodes
{
    public class HasJudgementThresholdNotExceededNode : BTNode
    {
        private AIBrain _brain;
        private int _thresholdIndex;


        public HasJudgementThresholdNotExceededNode(AIBrain brain, int thresholdIndex)
        {
            _brain = brain;
            _thresholdIndex = thresholdIndex;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if (!_brain.RuntimeData.HasJudgementTimerOfIndexExceeded(_thresholdIndex))
                return NodeState.SUCCESS;
            else
                return NodeState.FAILURE;
        }
    }
}
