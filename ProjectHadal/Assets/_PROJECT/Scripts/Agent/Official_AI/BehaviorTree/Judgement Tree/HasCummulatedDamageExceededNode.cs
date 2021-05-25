namespace Hadal.AI.TreeNodes
{
    public class Node : BTNode
    {
        private AIBrain _brain;

        public Node(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
