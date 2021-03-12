using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                                .WithOnCompleteEvent(() => returnToDefaultState = true)
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
    
            $"stunned".Msg();
        }
        public void StateTick()
        {

        }
        public void OnStateEnd()
        {
            returnToDefaultState = false;
            onThisState = false;
            stunTimer.Pause();
            Brain.SetIsStunned(false);
            $"no longer stunned".Msg();
        }
        public Func<bool> ShouldTerminate() => () => returnToDefaultState;
    }
}
