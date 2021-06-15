using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.States;
using Hadal.AI.Caverns;

//! C: Jon
namespace Hadal.AI.States
{
    public class RecoveryState : AIStateBase
    {
        RecoveryStateSettings settings;
        CavernHandler targetCavern;

        public RecoveryState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Recovery;
        }

        public override void OnStateStart()
		{
			if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            RuntimeData.UpdateCumulativeDamageThreshold(settings.GetEscapeDamageThreshold(Brain.HealthManager.GetCurrentHealth));
            SetNewTargetCavern();
            AllowStateTick = true;
        }

        public override void StateTick() 
        {
            if (!AllowStateTick) return;

            RuntimeData.TickRecoveryTicker(Time.deltaTime);

            //! When hit too much or time too long
            if (RuntimeData.GetRecoveryTicks >= settings.MaxEscapeTime || RuntimeData.HasCumulativeDamageExceeded)
            {
                RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                RuntimeData.SetBrainState(BrainState.Engagement);
                RuntimeData.ResetRecoveryTicker();
                AllowStateTick = false;
            }
        }

        public override void LateStateTick()
        {
        }
        public override void FixedStateTick()
        {
        }
        public override void OnStateEnd() { }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            if (cavern == targetCavern)
            {
                if (cavern.GetPlayerCount > 1) SetNewTargetCavern();
                else if (cavern.GetPlayerCount == 1 && Brain.CarriedPlayer != null)
                {
                    if (cavern.GetPlayersInCavern[0] == Brain.CarriedPlayer)
                    {
                        //! TODO: Thresh player
                    }
                }
            }
        }

        public override void OnPlayerEnterAICavern(CavernPlayerData data)
        {
            SetNewTargetCavern();
        }

        #region State specific
        void SetNewTargetCavern()
        {
            targetCavern = Brain.CavernManager.GetLeastPopulatedCavern(Brain.CavernManager.GetHandlerListExcludingAI());
            NavigationHandler.SetImmediateDestinationToCavern(targetCavern);
        }
        #endregion

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
