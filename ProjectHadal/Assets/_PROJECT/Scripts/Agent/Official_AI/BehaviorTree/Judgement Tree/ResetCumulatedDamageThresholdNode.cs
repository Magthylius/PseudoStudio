using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class ResetCumulatedDamageThresholdNode : BTNode
    {
        private AIBrain _brain;

        public ResetCumulatedDamageThresholdNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
            //_brain.RuntimeData.UpdateCumulativeDamageThreshold(_brain.HealthManager.GetCurrentHealth);
            return NodeState.SUCCESS;
        }
    }
}
