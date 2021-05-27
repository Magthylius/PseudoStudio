using Tenshi;

namespace Hadal.AI.TreeNodes
{
    public class ModifyConfidenceNode : BTNode
    {
        private AIBrain _brain;
        private int _modifyDifference;

        public ModifyConfidenceNode(AIBrain brain, int modifyAmount, bool isAddition)
        {
            _brain = brain;
            modifyAmount = modifyAmount.Abs();
            if (!isAddition)
                modifyAmount = -modifyAmount;
            
            _modifyDifference = modifyAmount;
        }

        public override NodeState Evaluate()
        {
            _brain.UpdateConfidenceValue(_modifyDifference);
            return NodeState.SUCCESS;
        }
    }
}
