using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.States;
using Hadal.AI.Caverns;

namespace Hadal.AI
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
            SetTargetEscapeCavern();
        }

        public override void StateTick() 
        {
            if (!AllowStateTick) return;

            RuntimeData.TickRecoveryTicker(Time.deltaTime);

            //! Damage threshold counter
            if (RuntimeData.GetRecoveryTicks >= settings.MaxEscapeTime || RuntimeData.HasCumulativeDamageExceeded)
            {
                //! Nigeru
                FailedEscape();
                
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
                if (cavern.GetPlayerCount > 1) SetTargetEscapeCavern();
                else if (cavern.GetPlayerCount == 1 && Brain.CarriedPlayer != null)
                {
                    if (cavern.GetPlayersInCavern[0] == Brain.CarriedPlayer)
                    {
                        //! TODO: Thresh player
                    }
                }
            }
        }

        #region State specific
        void FailedEscape()
        {
            RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
            RuntimeData.SetMainObjective(MainObjective.Engagement);
        }

        void SetTargetEscapeCavern()
        {
            targetCavern = Brain.CavernManager.GetLeastPopulatedCavern(Brain.CavernManager.GetHandlerListExcludingAI());
        }
        #endregion

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
