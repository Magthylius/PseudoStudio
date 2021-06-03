using Tenshi.AIDolls;
using Hadal.Utility;
using Tenshi.UnitySoku;
using System;

namespace Hadal.AI
{
    public class StunnedState : IState
    {
        public AIBrain Brain { get; private set; }
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
        public void OnStateStart()
        {
            returnToDefaultState = false;
            onThisState = true;
            stunTimer.Restart();
            Brain.SetIsStunned(false);
        }
        public void StateTick()
        {

        }
        public void LateStateTick()
        {
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd()
        {
            returnToDefaultState = false;
            onThisState = false;
            stunTimer.Pause();

        }
        public Func<bool> ShouldTerminate() => () => returnToDefaultState;
    }
}
