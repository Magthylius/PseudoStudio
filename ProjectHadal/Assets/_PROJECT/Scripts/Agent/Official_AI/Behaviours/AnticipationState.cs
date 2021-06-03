using Tenshi.AIDolls;
using System;

namespace Hadal.AI
{
    public class AnticipationState : IState
    {
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;
        
        public AnticipationState(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
        }

        public void OnStateStart() { }
        public void StateTick()
        {
            MainObjective objective = MainObjective.None;
            
            //! Anticipation evaluation here
            // ...

            // if (ambush)
            //     objective = AnticipationObjective.Ambush;
            // else if (aggressive)
            //     objective = AnticipationObjective.Aggressive;
            
            Brain.RuntimeData.SetMainObjective(objective);
        }
        public void LateStateTick() { }
        public void FixedStateTick() { }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;
    }
}
