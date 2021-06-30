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
        AnticipationStateSettings settings;

        //! Meant just for startup
        private bool gameStartupInitialization = false;

        public AnticipationState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Anticipation;

            //Debug.LogWarning("heyheyinit");
            GameManager.Instance.GameStartedEvent += StartInitialization;
        }

        public override void OnStateStart()
        {
            RuntimeData.ResetAnticipationTicker();

            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            NavigationHandler.SetCanPath(true);

            //Brain.StartCoroutine(InitializeAfterCaverns());


            //print(RuntimeData.GetEngagementObjective);
            // if (RuntimeData.GetEngagementObjective == EngagementSubState.Judgement || RuntimeData.GetEngagementObjective == EngagementSubState.None)
            //     ForceEngagementObjective(EngagementSubState.Aggressive);


        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            /*if (!Brain.IsStunned)
            {
                RuntimeData.TickAnticipationTicker(Time.deltaTime);
                Brain.RuntimeData.SetBrainState(BrainState.Engagement);
                Brain.RuntimeData.SetEngagementSubState(RuntimeData.GetEngagementObjective);
            }*/
        }

        public override void LateStateTick()
        {
        }

        public override void FixedStateTick()
        {
            if (!AllowStateTick) return;

            if (Brain.CurrentTarget != null)
            {
                RuntimeData.TickAnticipationTicker(Time.deltaTime);
                Brain.RuntimeData.SetBrainState(BrainState.Engagement);
                //Brain.RuntimeData.SetEngagementSubState(RuntimeData.GetEngagementObjective);
                Brain.RuntimeData.SetEngagementSubState(EngagementSubState.Judgement);
                if (Brain.DebugEnabled) Debug.Log("Spotted and entered engagement!");
            }
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
                    //Brain.RuntimeData.SetEngagementSubState(RuntimeData.GetEngagementObjective);
                    Brain.RuntimeData.SetEngagementSubState(EngagementSubState.Judgement);
                    if (Brain.DebugEnabled) Debug.Log("Anticipation => Engagement");
                }
            }
            else if (gameStartupInitialization) DetermineNextCavern();
        }

        void StartInitialization()
        {
            //.LogWarning("heyheyxd");
            Brain.StartCoroutine(InitializeAfterCaverns());
        }


        IEnumerator InitializeAfterCaverns()
        {
            //Debug.LogWarning("heyhey0");

            //! Wait for caverns to init
            while (!CavernManager.CavernsInitialized)
            {
                yield return null;
            }

            //Debug.LogWarning("heyhey1");

            SetNewTargetCavern();
            if (Brain.TargetMoveCavern == null)
            {
                //! Check if game ended
                AllowStateTick = false;
                //return;
                yield return null;
            }

            //Debug.LogWarning("heyhey2");

            AllowStateTick = true;
            RuntimeData.SetEngagementSubState(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));

            gameStartupInitialization = true;
            DetermineNextCavern();
            //Debug.LogWarning("heyhey3");

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

                    if (targetCavern == null)
                    {
                        TunnelBehaviour targetTunnel = CavernManager.GetMostPopulatedTunnel();

                        if (targetTunnel != null)
                        {
                            targetCavern = CavernManager.GetRandomCavernExcluding(targetTunnel.GetConnectedCaverns(), AICavern);
                        }
                        else
                        {
                            Debug.LogWarning("Cannot find any players at all! Has the game ended?");
                            targetCavern = CavernManager.GetRandomCavern();
                        }
                    }
                    //print(targetCavern);
                    break;
                case EngagementSubState.Ambush:
                    if (Brain.DebugEnabled) print("Anticipation: Ambush.");
                    targetCavern = CavernManager.GetLeastPopulatedCavern(CavernManager.GetMostPopulatedCavern().ConnectedCaverns);
                    break;
                default:
                    break;
            }

            if (targetCavern == null)
            {
                Debug.LogError("Random target cavern fallback! Something went wrong!");
                targetCavern = CavernManager.GetRandomCavern();
            }

            //targetCavern = CavernManager.GetCavern(CavernTag.Starting);
            //print(targetCavern);
            Brain.UpdateTargetMoveCavern(targetCavern);
            CavernManager.SeedCavernHeuristics(targetCavern);
        }

        void DetermineNextCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, RuntimeData.GetEngagementObjective != EngagementSubState.Aggressive);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
            //NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
            Brain.UpdateNextMoveCavern(nextCavern);

            if (Brain.DebugEnabled) "Determining Next Cavern".Msg();
        }

        void ForceEngagementObjective(EngagementSubState newObjective) => RuntimeData.SetEngagementSubState(newObjective);
        public override Func<bool> ShouldTerminate() => () => false;
    }
}