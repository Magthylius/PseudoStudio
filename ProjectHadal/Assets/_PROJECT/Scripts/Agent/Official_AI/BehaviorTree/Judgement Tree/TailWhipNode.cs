using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class TailWhipNode : BTNode
    {
        private AIBrain _brain;
        private bool _tailWhipDone;

        public TailWhipNode(AIBrain brain)
        {
            _brain = brain;
            _tailWhipDone = false;
        }

        public override NodeState Evaluate()
        {
            // trigger tailwhip code here
            _tailWhipDone = true;

            if (!_tailWhipDone)
                return NodeState.RUNNING;

            "Whips something?".Msg();
            return NodeState.SUCCESS;
        }
    }
}
