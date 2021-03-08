using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;

namespace Hadal.AI
{
    public class AnticipationState : IState
    {
        public void OnStateStart() { }
        public void StateTick() { }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;
    }

}
