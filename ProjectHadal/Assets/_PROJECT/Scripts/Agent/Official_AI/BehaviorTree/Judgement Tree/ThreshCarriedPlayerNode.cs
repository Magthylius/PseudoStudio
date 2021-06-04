using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class ThreshCarriedPlayerNode : BTNode
    {
        private AIBrain _brain;

        public ThreshCarriedPlayerNode(AIBrain brain)
        {
            _brain = brain;
        }

        public override NodeState Evaluate()
        {
			if (_brain.RuntimeData.CurrentTarget == null)
				return NodeState.FAILURE;
			
            "AI: I am hurting the player".Bold().Msg();
            return NodeState.RUNNING;
        }
    }
}
