using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class LureState : AIStateBase
    {
        private IEnumerator debugRoutine;
        LureStateSettings settings;
        public LureState(AIBrain brain)
        {
            Initialize(brain);
            debugRoutine = null;
            settings = MachineData.Lure;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            NavigationHandler.OnReachedPoint += StartSwitchObjective;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;
        }

        public override void LateStateTick()
        {
        }

        public override void FixedStateTick()
        {
        }

        public override void OnStateEnd()
        {
            NavigationHandler.OnReachedPoint -= StartSwitchObjective;
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            if (Brain.StateSuspension) return;
            CavernManager.SeedCavernHeuristics(cavern);
            DetermineNextCavern();
        }

        void DetermineNextCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
            Brain.UpdateNextMoveCavern(nextCavern);

            if (Brain.DebugEnabled) "Determining Next Cavern".Msg();
        }

        void StartSwitchObjective()
        {
            Brain.StartCoroutine(SwitchObjective(BrainState.Anticipation));
        }

        IEnumerator SwitchObjective(BrainState newObjective)
        {
            float sqrCloseDistance = 100f.Sqr();
            while (NavigationHandler.GetCurrentPoint.GetSqrDistanceTo(Brain.transform.position) > sqrCloseDistance)
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(settings.StateExitDelay);
            NavigationHandler.StopCustomPath();
            RuntimeData.SetBrainState(newObjective);
        }
    }
}
