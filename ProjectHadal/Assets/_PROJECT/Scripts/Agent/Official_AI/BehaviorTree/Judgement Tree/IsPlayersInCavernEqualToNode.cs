using Hadal.AI.Caverns;
using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class IsPlayersInCavernEqualToNode : BTNode
    {
        private AIBrain _brain;
        private int _checkCount;
        private CavernTag _currentCavern;

        public IsPlayersInCavernEqualToNode(AIBrain brain, int numberOfPlayersToCheck)
        {
            _brain = brain;
            _checkCount = numberOfPlayersToCheck;
            _currentCavern = CavernTag.Invalid;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            //! Identify where the AI is
            _currentCavern = _brain.CavernManager.GetCavernTagOfAILocation();
            if (_currentCavern == CavernTag.Invalid) return NodeState.FAILURE;

            //! Get the cavern handler of the location
            CavernHandler handler = _brain.CavernManager.GetCavern(_currentCavern);
            if (handler == null) return NodeState.FAILURE;
            
            //! Identify how many players are in the location
            int playerCount = handler.GetPlayerCount;
            if (playerCount != _checkCount) return NodeState.FAILURE;

            return NodeState.SUCCESS;
        }
    }
}
