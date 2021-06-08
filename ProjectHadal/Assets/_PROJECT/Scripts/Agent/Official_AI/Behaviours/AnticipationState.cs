using Tenshi.AIDolls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

//! C: jet, E: jon
namespace Hadal.AI.States
{
    public class AnticipationState : AIStateBase
    {
        private IEnumerator debugRoutine;

        AnticipationStateSettings settings;

        CavernHandler targetCavern;

        public AnticipationState(AIBrain brain)
        {
            Initialize(brain);
            debugRoutine = null;
            settings = MachineData.Anticipation;
        }

        IEnumerator Debug_SwitchToEngagementJudgementState()
        {
            yield return new WaitForSeconds(2f);
            Brain.RuntimeData.SetMainObjective(MainObjective.Engagement);
            Brain.RuntimeData.SetEngagementObjective(EngagementObjective.Judgement);
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            print("Entered anticipation!");
            
            //if (debugRoutine != null) return;
            //debugRoutine = Debug_SwitchToEngagementJudgementState();
            //Brain.StartCoroutine(debugRoutine);

            //targetCavern = Brain.CavernManager.GetMostPopulatedCavern();

            SetNewTargetCavern();
            if (targetCavern == null)
            {
                //! Check if game ended
                AllowStateTick = false;
                return;
            }

            AllowStateTick = true;
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));
            
            
            Brain.StartCoroutine(CheckPlayersInRange());
            
        }

        IEnumerator DebugRoutine()
        {
            yield return new WaitForSeconds(5f);

        }

        public override void StateTick()
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

            if (!AllowStateTick) return;
            //! Move to target cavern
            //! Check if players in range/damaged by player
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

        public override void OnCavernEnter(CavernHandler cavern)
        {
            print("Cavern entered");
            DetermineNextCavern();
        }

        IEnumerator CheckPlayersInRange()
        {
            yield return new WaitForSeconds(0.2f);
        }

        void SetNewTargetCavern()
        {
            EngagementObjective currentObj = RuntimeData.GetEngagementObjective;

            switch (currentObj)
            {
                case EngagementObjective.Aggressive:
                    if (Brain.DebugEnabled) print("Anticipation: Aggressive.");
                    targetCavern = CavernManager.GetMostPopulatedCavern();
                    break;
                case EngagementObjective.Ambush:
                    if (Brain.DebugEnabled) print("Anticipation: Ambush.");
                    targetCavern = CavernManager.GetLeastPopulatedCavern(CavernManager.GetMostPopulatedCavern().ConnectedCaverns);
                    break;
                default:
                    Debug.LogError("Incorrect engagement objective!");
                    break;
            }

            targetCavern = CavernManager.GetCavern(CavernTag.Starting);
        }

        void DetermineNextCavern()
        {
            CavernHandler nextCavern =
                CavernManager.GetLeastPopulatedCavern(CavernManager.GetNextCaverns(targetCavern,
                    CavernManager.GetHandlerOfAILocation));
            
            print(nextCavern);
            NavigationHandler.SetTargetNavPointAtCavern(nextCavern);
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}