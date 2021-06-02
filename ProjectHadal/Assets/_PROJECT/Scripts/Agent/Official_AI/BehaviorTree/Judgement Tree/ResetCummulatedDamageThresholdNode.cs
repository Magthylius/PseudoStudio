using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class ResetCummulatedDamageThresholdNode : BTNode
    {
        private AIBrain _brain;

        public ResetCummulatedDamageThresholdNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            _brain.ResetCummulativeDamageThreshold();
            return NodeState.SUCCESS;
        }
    }
}
