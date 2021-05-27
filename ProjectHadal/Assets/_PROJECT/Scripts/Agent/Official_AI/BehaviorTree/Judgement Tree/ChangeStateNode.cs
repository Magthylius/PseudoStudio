namespace Hadal.AI.TreeNodes
{
    public class ChangeStateNode : BTNode
    {
        private AIBrain _brain;
        private Objective _stateObjective;

        public ChangeStateNode(AIBrain brain, Objective objective)
        {
            _brain = brain;
            _stateObjective = objective;
        }

        public override NodeState Evaluate()
        {
            _brain.SetObjective(_stateObjective);
            return NodeState.SUCCESS;
        }
    }
}
