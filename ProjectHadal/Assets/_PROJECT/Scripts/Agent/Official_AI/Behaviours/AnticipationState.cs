using Tenshi.AIDolls;
using System;
using System.Collections;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.States;
using Hadal.AI.Caverns;

//! C: jet, E: jon
namespace Hadal.AI
{
    public class AnticipationState : IState
    {
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;
		private IEnumerator debugRoutine;

		LeviathanRuntimeData runtimeData;
		StateMachineData machineData;

		CavernHandler targetCavern;
		CavernHandler nextCavern;

		bool allowStateTick = true;
        
        public AnticipationState(AIBrain brain)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
			debugRoutine = null;
        }
		
		IEnumerator Debug_SwitchToEngagementJudgementState()
		{
			yield return new WaitForSeconds(2f);
			Brain.RuntimeData.SetMainObjective(MainObjective.Engagement);
			Brain.RuntimeData.SetEngagementObjective(EngagementObjective.Judgement);
		}

        public void OnStateStart()
		{
			if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
			NavigationHandler.SetCanPath(true);
			
			if (debugRoutine != null) return;
			debugRoutine = Debug_SwitchToEngagementJudgementState();
			Brain.StartCoroutine(debugRoutine);

			targetCavern = Brain.CavernManager.GetMostPopulatedCavern();

			if (targetCavern == null)
            {
				//! Check if game ended
				allowStateTick = false;
				return;
            }

			allowStateTick = true;
			runtimeData = Brain.RuntimeData;
			machineData = Brain.MachineData;
			runtimeData.SetEngagementObjective(machineData.Anticipation.GetRandomInfluencedObjective(runtimeData.NormalisedConfidence));
		}
		
        public void StateTick()
        {
			//! Anticipation evaluation here
			// ...

			/*
			EngagementObjective eObj = Brain.MachineData.Anticipation.GetClearObjective(Brain.RuntimeData.NormalisedConfidence);
			if (eObj != EngagementObjective.None)
			{
				LeviathanRuntimeData d = Brain.RuntimeData;
				d.SetMainObjective(MainObjective.Engagement);
				d.SetEngagementObjective(eObj);
			}*/

			if (!allowStateTick) return;


        }
        public void LateStateTick() { }
        public void FixedStateTick() { }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;
    }
}
