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
        IdleStateSettings settings;

        public IdleState(AIBrain brain)
        {
            Initialize(brain);
            debugRoutine = null;
            settings = MachineData.Idle;
        }

        public override void OnStateStart()
        {

            RuntimeData.ResetIdleTicker();

            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            Brain.StartCoroutine(InitAfterCaverns());
            
            GameManager.Instance.GameStartedEvent += StartSwitchObjective;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (!Brain.IsStunned)
                RuntimeData.TickIdleTicker(Time.deltaTime);
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

        void StartSwitchObjective(bool booleanData)
        {
            Brain.StartCoroutine(SwitchObjective(BrainState.Anticipation));
        }

        IEnumerator SwitchObjective(BrainState newObjective)
        {
            yield return new WaitForSeconds(settings.StateExitDelay);
            RuntimeData.SetBrainState(newObjective);
        }

        IEnumerator InitAfterCaverns()
        {
            //! Wait for caverns to init
            while (!CavernManager.CavernsInitialized)
            {
                yield return null;
            }

            //LingerAroundCavern();
        }

        void LingerAroundCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true, true);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
        }
    }
}
