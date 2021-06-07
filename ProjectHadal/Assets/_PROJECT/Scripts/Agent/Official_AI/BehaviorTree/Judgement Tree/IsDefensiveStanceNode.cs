namespace Hadal.AI.TreeNodes
{
    public class IsDefensiveStanceNode : BTNode
    {
        private AIBrain _brain;

        public IsDefensiveStanceNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            return default;
        }
    }
}
