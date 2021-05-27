namespace Hadal.AI.TreeNodes
{
    public class IsCarryingAPlayerNode : BTNode
    {
        private AIBrain _brain;

        public IsCarryingAPlayerNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
