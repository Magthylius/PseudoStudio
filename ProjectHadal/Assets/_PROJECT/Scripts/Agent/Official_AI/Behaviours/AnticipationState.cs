using Tenshi.AIDolls;
using System;
using System.Collections;
using UnityEngine;

namespace Hadal.AI
{
    public class AnticipationState : IState
    {
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;
		private IEnumerator debugRoutine;
        
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
			NavigationHandler.SetCanPath(true);
			
			if (debugRoutine != null) return;
			debugRoutine = Debug_SwitchToEngagementJudgementState();
			Brain.StartCoroutine(debugRoutine);
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
        }
        public void LateStateTick() { }
        public void FixedStateTick() { }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;
    }
}
