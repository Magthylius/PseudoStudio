namespace Hadal.AI.TreeNodes
{
    public class ChangeStateNode : BTNode
    {
        private AIBrain _brain;
        private MainObjective _stateObjective;

        public ChangeStateNode(AIBrain brain, MainObjective objective)
        {
            _brain = brain;
            _stateObjective = objective;
        }

        public override NodeState Evaluate()
        {
            _brain.RuntimeData.SetMainObjective(_stateObjective);
            return NodeState.SUCCESS;
        }
    }
}
