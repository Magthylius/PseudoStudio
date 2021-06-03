namespace Hadal.AI.TreeNodes
{
    public class HasCumulatedDamageExceededNode : BTNode
    {
        private AIBrain _brain;

        public HasCumulatedDamageExceededNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            if (_brain.RuntimeData.HasCumulativeDamageExceeded())
                return NodeState.SUCCESS;
            return NodeState.FAILURE;
        }
    }
}
