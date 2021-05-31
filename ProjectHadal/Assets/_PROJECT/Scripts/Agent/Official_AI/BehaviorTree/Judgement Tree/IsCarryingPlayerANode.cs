namespace Hadal.AI.TreeNodes
{
    public class IsCarryingAPlayerNode : BTNode
    {
        private AIBrain _brain;
        private bool _checkForTargetPlayer;

        public IsCarryingAPlayerNode(AIBrain brain, bool checkForTargetPlayer)
        {
            _brain = brain;
            _checkForTargetPlayer = checkForTargetPlayer;
        }

        public override NodeState Evaluate()
        {
            if (_checkForTargetPlayer)
            {
                if (_brain.CarriedPlayer == _brain.TargetingPlayer)
                    return NodeState.SUCCESS;
                return NodeState.FAILURE;
            }
            
            if (_brain.CarriedPlayer != null)
                return NodeState.SUCCESS;
            return NodeState.FAILURE;
        }
    }
}
