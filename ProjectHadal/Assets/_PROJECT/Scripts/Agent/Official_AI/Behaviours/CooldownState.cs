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
        CavernHandler targetCavern;

        public CooldownState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Cooldown;
        }

        public override void OnStateStart()
        {
            //! Change speed
            NavigationHandler.SetSpeedMultiplier(settings.ElusiveSpeedModifier);
            SetNewTargetCavern();
            AllowStateTick = true;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

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
            targetCavern = Brain.CavernManager.GetLeastPopulatedCavern();
            NavigationHandler.SetDestinationToCavern(null, targetCavern);
        }
    }
}
