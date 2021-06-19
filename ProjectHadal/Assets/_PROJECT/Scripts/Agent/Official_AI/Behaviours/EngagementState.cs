using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using Hadal.Networking;
using Debug = UnityEngine.Debug;
using Photon.Realtime;

namespace Hadal.AI.States
{
    public class EngagementState : AIStateBase
    {
        private StateMachine subStateMachine;

        public EngagementState(AIBrain brain, AggressiveSubState aggressive, AmbushSubState ambush, JudgementSubState judgement)
        {
            Initialize(brain);
            

            //! intialise sub machine and states
            aggressive.SetParent(this);
			ambush.SetParent(this);
			judgement.Initialise(this);

            subStateMachine = new StateMachine();
            subStateMachine.AddEventTransition(to: ambush, withCondition: OnAmbush());
            subStateMachine.AddEventTransition(to: aggressive, withCondition: OnAggressive());
            subStateMachine.AddEventTransition(to: judgement, withCondition: OnJudgement());
            
            subStateMachine.SetState(judgement); // default state

            //! transition conditions
            Func<bool> OnAmbush() => () => Brain.RuntimeData.GetEngagementObjective == EngagementSubState.Ambush;
            Func<bool> OnAggressive() => () => Brain.RuntimeData.GetEngagementObjective == EngagementSubState.Aggressive;
            Func<bool> OnJudgement() => () => Brain.RuntimeData.GetEngagementObjective == EngagementSubState.Judgement;
        }
        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            RuntimeData.SetEngagementSubState(EngagementSubState.Judgement);
            subStateMachine.CurrentState.OnStateStart();
        }
        public override void StateTick()
        {
            subStateMachine.MachineTick();
        }
        public override void LateStateTick()
        {
            subStateMachine.LateMachineTick();
        }
        public override void FixedStateTick()
        {
            subStateMachine.FixedMachineTick();
        }
        public override void OnStateEnd()
        {
            subStateMachine.CurrentState.OnStateEnd();
            Brain.RuntimeData.SetEngagementSubState(EngagementSubState.None);
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            Brain.UpdateTargetMoveCavern(AICavern);
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
