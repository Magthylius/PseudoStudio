using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.AStarPathfinding;
using System;
using System.Threading.Tasks;
using System.Linq;
using Hadal.AI.GeneratorGrid;

namespace Hadal.AI.States
{
    public class IdleState : IState
    {
        #region Variables
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;

        public bool IsCurrentState { get; set; }

        #endregion

        public IdleState(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
        }
        public void OnStateStart()
        {
            NavigationHandler.SetCanPath(true);
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
            
        }

        public Func<bool> ShouldTerminate() => () => false;
    }
}
