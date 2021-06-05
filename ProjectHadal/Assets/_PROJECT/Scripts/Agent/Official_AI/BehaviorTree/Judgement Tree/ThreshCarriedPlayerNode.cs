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
			if (_brain.CarriedPlayer == null)
				return NodeState.FAILURE;
            // if (_brain.CarriedPlayer.GetInfo.HealthManager.)
			
            "AI: I am hurting the player".Bold().Msg();
            return NodeState.RUNNING;
        }
    }
}
