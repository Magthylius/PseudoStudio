using Hadal.AI.Caverns;
using Hadal.AI.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    public class CooldownState : AIStateBase
    {
        CooldownStateSettings settings;

        public CooldownState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Cooldown;
        }

        public override void OnStateStart()
        {
            //! Change speed
            NavigationHandler.SetSpeedMultiplier(settings.ElusiveSpeedModifier);
            RuntimeData.ResetCooldownTicker();
            HealthManager.SetIgnoreSlowDebuffs(true);
            SetNewTargetCavern();
            AllowStateTick = true;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (!Brain.IsStunned)
                RuntimeData.TickCooldownTicker(Time.deltaTime);

            if (RuntimeData.GetCooldownTicks >= settings.MaxCooldownTime)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
                RuntimeData.ResetCooldownTicker();
                AllowStateTick = false;
            }
        }

        public override void OnStateEnd()
        {
            //! Reset speed
            NavigationHandler.ResetSpeedMultiplier();
            HealthManager.SetIgnoreSlowDebuffs(false);
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            if (cavern.GetPlayerCount > 0)
                SetNewTargetCavern();
        }

        public override void OnPlayerEnterAICavern(CavernPlayerData data)
        {
            SetNewTargetCavern();
        }

        void SetNewTargetCavern()
        {
            CavernHandler targetCavern = Brain.CavernManager.GetLeastPopulatedCavern(Brain.CavernManager.GetHandlerListExcludingAI());
            Brain.UpdateTargetMoveCavern(targetCavern);

            CavernManager.SeedCavernHeuristics(targetCavern);
            DetermineNextCavern();
        }

        void DetermineNextCavern()
        {
            Brain.StartCoroutine(WaitForAICavern());
            //print(AICavern);
            IEnumerator WaitForAICavern()
            {
                while (AICavern == null)
                    yield return null;

                CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true);

                NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
                NavigationHandler.EnableCachedQueuePathTimer();
                Brain.UpdateNextMoveCavern(nextCavern);
            }
        }
    }
}
