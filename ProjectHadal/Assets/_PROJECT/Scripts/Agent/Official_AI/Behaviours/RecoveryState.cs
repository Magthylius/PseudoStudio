using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.States;

namespace Hadal.AI
{
    public class RecoveryState : AIStateBase
    {
        RecoveryStateSettings settings;

        public RecoveryState(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
            RuntimeData = Brain.RuntimeData;
        }

        public override void OnStateStart()
		{
			if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
		}
        public override void StateTick() { }
        public override void LateStateTick()
        {
        }
        public override void FixedStateTick()
        {
        }
        public override void OnStateEnd() { }
        public override Func<bool> ShouldTerminate() => () => false;
    }
}
