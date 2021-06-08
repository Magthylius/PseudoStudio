using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;
using Hadal.AI.Caverns;
using Hadal.AI.States;
using Hadal.Player;
using Tenshi;

namespace Hadal.AI
{
    public class AIStateBase : IState
    {
        public AIBrain Brain;
        public PointNavigationHandler NavigationHandler;
        public LeviathanRuntimeData RuntimeData;
        public StateMachineData MachineData;
        public CavernManager CavernManager;

        public bool AllowStateTick = true;

        public void Initialize(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
            RuntimeData = Brain.RuntimeData;
            MachineData = Brain.MachineData;
            CavernManager = Brain.CavernManager;
        }

        public virtual void FixedStateTick() { }

        public virtual void LateStateTick() { }

        public virtual void OnStateEnd() { }

        public virtual void StateTick() { }

        public virtual void OnStateStart() { }

        public virtual void OnCavernEnter(CavernHandler cavern) { }

        public virtual void OnPlayerEnterAICavern(CavernPlayerData data) { }

        public bool CheckGameHasEnded() { return false; }

        public bool IsCurrentState { get; set; } = false;

        public virtual Func<bool> ShouldTerminate() => () => false;

        protected void print(object message) => Debug.Log(message);
    }
}
