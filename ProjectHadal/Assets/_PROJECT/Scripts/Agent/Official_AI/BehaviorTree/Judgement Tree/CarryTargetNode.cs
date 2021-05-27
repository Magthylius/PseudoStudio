namespace Hadal.AI.TreeNodes
{
    public class CarryTargetNode : BTNode
    {
        private AIBrain _brain;
        private float _succeedDistance;
        private float _waitTime;

        public CarryTargetNode(AIBrain brain, float carrySucceedDistance, float carryWaitTime)
        {
            _brain = brain;
            _succeedDistance = carrySucceedDistance;
            _waitTime = carryWaitTime;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
