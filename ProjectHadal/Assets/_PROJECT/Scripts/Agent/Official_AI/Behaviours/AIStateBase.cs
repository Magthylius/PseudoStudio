using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;

namespace Hadal.AI
{
    public class AIStateBase : IState
    {
        public AIBrain Brain;
        public PointNavigationHandler NavigationHandler;

        public virtual void FixedStateTick() { }

        public virtual void LateStateTick() { }

        public virtual void OnStateEnd() { }

        public virtual void StateTick() { }

        public virtual void OnStateStart() { }

        public virtual void OnCavernEnter() { }

        public virtual Func<bool> ShouldTerminate() => () => false;
    }
}
