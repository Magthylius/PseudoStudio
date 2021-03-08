using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Tenshi.AIDolls;
using System;
using Hadal.AI.AStarPathfinding;
using Tenshi.UnitySoku;
using Hadal.AI.GeneratorGrid;

namespace Hadal.AI
{
    public class EngagementState : IState
    {
        StateMachine subStateMachine; //?
        SubStateState subStateState;
        float fightTimer = 0f;
        bool bfightTimer;

        public EngagementState()
        {

        }
        public void OnStateStart()
        {
            bfightTimer = true;
            "I am engaging now".Msg();
        }
        public void StateTick()
        {
            if(bfightTimer)
            {
                fightTimer += Time.deltaTime;
            }
        }
        public void OnStateEnd()
        {
            "I am exiting engage".Msg();
        }
        public Func<bool> ShouldTerminate() => () => false;

        enum SubStateState
        {
            Aggresive,
            Ambush,
            Judgement,
            Total
        }
    }
}
