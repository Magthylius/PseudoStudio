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

        //! Meant just for startup
        private bool gameStartupInitialization = false;

        public AnticipationState(AIBrain brain)
        {
            Initialize(brain);
            debugRoutine = null;
            settings = MachineData.Anticipation;
        }

        public override void OnStateStart()
        {
            RuntimeData.ResetAnticipationTicker();
            
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            Brain.StartCoroutine(InitializeAfterCaverns());
            
            //print(RuntimeData.GetEngagementObjective);
            if (RuntimeData.GetEngagementObjective == EngagementSubState.Judgement || RuntimeData.GetEngagementObjective == EngagementSubState.None)
                ForceEngagementObjective(EngagementSubState.Aggressive);
        }

        public override void StateTick()
        {
            RuntimeData.TickAnticipationTicker(Time.deltaTime);

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
            if (Brain.StateSuspension) return;

            if (cavern == Brain.TargetMoveCavern)
            {
                if (cavern.GetPlayerCount <= 0)
                    SetNewTargetCavern();
                else
                {
                    Brain.RuntimeData.SetBrainState(BrainState.Engagement);
                    Brain.RuntimeData.SetEngagementSubState(RuntimeData.GetEngagementObjective);
                }
            }
            else if (gameStartupInitialization) DetermineNextCavern();
        }

        IEnumerator CheckPlayersInRange()
        {
            yield return new WaitForSeconds(0.2f);
        }

        IEnumerator InitializeAfterCaverns()
        {
            //! Wait for caverns to init
            while (!CavernManager.CavernsInitialized)
            {
                yield return null;
            }
            
            SetNewTargetCavern();
            if (Brain.TargetMoveCavern == null)
            {
                //! Check if game ended
                AllowStateTick = false;
                //return;
                yield return null;
            }

            AllowStateTick = true;
            RuntimeData.SetEngagementSubState(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));
            
            Brain.StartCoroutine(CheckPlayersInRange());
            gameStartupInitialization = true;
            DetermineNextCavern();
        }
        
        void SetNewTargetCavern()
        {
            EngagementSubState currentObj = RuntimeData.GetEngagementObjective;
            CavernHandler targetCavern = null;

            //! Catch the engagement substate first
            if (RuntimeData.GetEngagementObjective == EngagementSubState.Judgement ||
                RuntimeData.GetEngagementObjective == EngagementSubState.None)
            {
                Debug.LogWarning("Incorrect engagement objective! Current objective: " + RuntimeData.GetEngagementObjective);
                ForceEngagementObjective(EngagementSubState.Aggressive);
                Debug.LogWarning("Forced objective to: " + RuntimeData.GetEngagementObjective);
            }
            
            switch (currentObj)
            {
                //! fall back to aggressive
                case EngagementSubState.None:
                case EngagementSubState.Judgement:
                    
                case EngagementSubState.Aggressive:
                    if (Brain.DebugEnabled) print("Anticipation: Aggressive.");
                    targetCavern = CavernManager.GetMostPopulatedCavern();
                    //print(targetCavern);
                    break;
                case EngagementSubState.Ambush:
                    if (Brain.DebugEnabled) print("Anticipation: Ambush.");
                    targetCavern = CavernManager.GetLeastPopulatedCavern(CavernManager.GetMostPopulatedCavern().ConnectedCaverns);
                    break;
                default:
                    break;
            }

            //targetCavern = CavernManager.GetCavern(CavernTag.Starting);
            //print(targetCavern);
            Brain.UpdateTargetMoveCavern(targetCavern);
            CavernManager.SeedCavernHeuristics(targetCavern);
        }

        void DetermineNextCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, RuntimeData.GetEngagementObjective != EngagementSubState.Aggressive);
           // NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            //NavigationHandler.EnableCachedQueuePathTimer();
            NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
            Brain.UpdateNextMoveCavern(nextCavern);
        }

        void ForceEngagementObjective(EngagementSubState newObjective) => RuntimeData.SetEngagementSubState(newObjective);
        public override Func<bool> ShouldTerminate() => () => false;
    }
}