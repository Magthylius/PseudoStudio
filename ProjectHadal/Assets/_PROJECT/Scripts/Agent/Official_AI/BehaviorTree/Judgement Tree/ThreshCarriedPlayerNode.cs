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
            "AI: I am hurting the player".Bold().Msg();
            return NodeState.RUNNING;
        }
    }
}
