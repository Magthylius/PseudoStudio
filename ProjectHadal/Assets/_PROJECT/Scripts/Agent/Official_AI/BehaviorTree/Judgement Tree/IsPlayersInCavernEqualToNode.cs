namespace Hadal.AI.TreeNodes
{
    public class IsPlayersInCavernEqualToNode : BTNode
    {
        private AIBrain _brain;
        private int _checkCount;

        public IsPlayersInCavernEqualToNode(AIBrain brain, int numberOfPlayersToCheck)
        {
            _brain = brain;
            _checkCount = numberOfPlayersToCheck;
        }

        public override NodeState Evaluate()
        {
            return default;
        }
    }
}
