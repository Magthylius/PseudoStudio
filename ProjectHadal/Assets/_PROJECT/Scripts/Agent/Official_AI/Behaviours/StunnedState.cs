using Hadal.Utility;
using Tenshi.UnitySoku;
using System;
using Tenshi;

namespace Hadal.AI
{
    public class StunnedState : AIStateBase
    {
        Timer stunTimer;
        bool returnToDefaultState = false;

        public StunnedState(AIBrain brain)
        {
            Brain = brain;
            returnToDefaultState = false;
            stunTimer = Brain.Create_A_Timer()
                                .WithDuration(Brain.stunDuration)
                                .WithOnCompleteEvent(CancelStun)
                                .WithShouldPersist(true);
            stunTimer.Pause();
        }
        ~StunnedState() => stunTimer.Destroy();
        
        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            returnToDefaultState = false;
            stunTimer.RestartWithDuration(Brain.stunDuration);
        }
        public override void StateTick() { }
        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd()
        {
            returnToDefaultState = false;
            stunTimer.Pause();
        }

        private void CancelStun()
        {
            returnToDefaultState = true;
            Brain.StopStun();
        }

        public override Func<bool> ShouldTerminate() => () => returnToDefaultState;
    }
}
