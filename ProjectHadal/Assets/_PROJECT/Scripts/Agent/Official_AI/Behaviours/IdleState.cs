using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class IdleState : AIStateBase
    {
        private IEnumerator debugRoutine;

        public IdleState(AIBrain brain)
        {
            Initialize(brain);
            debugRoutine = null;
        }

        public override void OnStateStart()
        {
            RuntimeData.ResetIdleTicker();

            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            Brain.StartCoroutine(InitAfterCaverns());


        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (!Brain.IsStunned)
                RuntimeData.TickIdleTicker(Time.deltaTime);

            if (RuntimeData.GetIdleTicks > 60)
            {
                SwitchObjective(BrainState.Anticipation);
            }

            Debug.LogWarning("IdleTick: " + RuntimeData.GetIdleTicks);
        }

        public override void LateStateTick()
        {
        }

        public override void FixedStateTick()
        {
        }

        public override void OnStateEnd()
        {
        }

        void SwitchObjective(BrainState newObjective) => RuntimeData.SetBrainState(newObjective);

        IEnumerator InitAfterCaverns()
        {
            //! Wait for caverns to init
            while (!CavernManager.CavernsInitialized)
            {
                yield return null;
            }

            LingerAroundCavern();
        }

        void LingerAroundCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true, true);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
        }
    }
}
