namespace Hadal.AI.TreeNodes
{
    public class TailWhipNode : BTNode
    {
        private AIBrain _brain;

        public TailWhipNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
