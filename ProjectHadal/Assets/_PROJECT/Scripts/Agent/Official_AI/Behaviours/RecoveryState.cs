using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI
{
    public class RecoveryState : IState
    {
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;

        public RecoveryState(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
        }

        public void OnStateStart()
		{
			if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
		}
        public void StateTick() { }
        public void LateStateTick()
        {
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;
    }
}
