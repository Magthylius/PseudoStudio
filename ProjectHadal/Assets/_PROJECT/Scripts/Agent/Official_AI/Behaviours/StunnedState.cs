using Tenshi.AIDolls;
using Hadal.Utility;
using Tenshi.UnitySoku;
using System;

namespace Hadal.AI
{
    public class StunnedState : AIStateBase
    {
        Timer stunTimer;
        bool returnToDefaultState = false;
        bool onThisState;

        public StunnedState(AIBrain brain)
        {
            Brain = brain;
            returnToDefaultState = false;
            stunTimer = Brain.Create_A_Timer()
                                .WithDuration(Brain.stunDuration)
                                .WithOnCompleteEvent(() =>                                
                                    returnToDefaultState = true)
                                .WithOnUpdateEvent(_ =>
                                {
                                    if (onThisState)
                                        $"Stun timer: {(100f * stunTimer.GetCompletionRatio):F2}%".Msg();

                                })
                                .WithShouldPersist(true);
            stunTimer.Pause();
        }
        public override void OnStateStart()
        {
            returnToDefaultState = false;
            onThisState = true;
            stunTimer.Restart();
            Brain.StopStun();
        }
        public override void StateTick()
        {

        }
        public override void LateStateTick()
        {
        }
        public override void FixedStateTick()
        {
        }
        public override void OnStateEnd()
        {
            returnToDefaultState = false;
            onThisState = false;
            stunTimer.Pause();

        }
        public override Func<bool> ShouldTerminate() => () => returnToDefaultState;
    }
}
