namespace Hadal.AI.TreeNodes
{
    public class ChangeStateNode : BTNode
    {
        private AIBrain _brain;
        private BrainState _stateObjective;

        public ChangeStateNode(AIBrain brain, BrainState objective)
        {
            _brain = brain;
            _stateObjective = objective;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            // return NodeState.SUCCESS;
            _brain.RuntimeData.SetBrainState(_stateObjective);
            return NodeState.SUCCESS;
        }
    }
}
