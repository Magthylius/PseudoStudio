namespace Hadal.AI.TreeNodes
{
    public class HasJudgementThresholdExceededNode : BTNode
    {
        private AIBrain _brain;

        public HasJudgementThresholdExceededNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
