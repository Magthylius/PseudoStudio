namespace Hadal.AI.TreeNodes
{
    public class IsAggressiveStanceNode : BTNode
    {
        private AIBrain _brain;

        public IsAggressiveStanceNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            return default;
        }
    }
}
