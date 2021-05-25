using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;

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

        public void OnStateStart() { }
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
