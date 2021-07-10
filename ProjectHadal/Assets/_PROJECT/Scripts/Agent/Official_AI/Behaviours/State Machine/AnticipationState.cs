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
        private float toGoAmbushTimer = 0f;

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
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            RuntimeData.ResetAnticipationTicker();
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));
            NavigationHandler.SetCanPath(true);

            toGoAmbushTimer = settings.ToGoAmbushTime;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (CheckForJudgementStateCondition()) return;
            if (CheckForAmbushStateCondition()) return;
        }

        public override void LateStateTick()
        {
            if (!AllowStateTick) return;
        }

        public override void FixedStateTick()
        {
            if (!AllowStateTick) return;
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
                    Brain.RuntimeData.SetBrainState(BrainState.Judgement);
                    if (Brain.DebugEnabled) Debug.Log("Anticipation => Engagement");
                }
            }
            else if (gameStartupInitialization) DetermineNextCavern();
        }

        void StartInitialization(bool booleanData)
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

            while (AICavern == null)
            {
                yield return null;
            }

            //Debug.LogWarning("heyhey2");

            AllowStateTick = true;
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));

            gameStartupInitialization = true;
            DetermineNextCavern();
            //Debug.LogWarning("heyhey3");

        }

        void SetNewTargetCavern()
        {
            EngagementObjective currentObj = RuntimeData.GetEngagementObjective;
            CavernHandler targetCavern = null;

            //! Catch the engagement substate first
            // if (RuntimeData.GetEngagementObjective == EngagementObjective.Judgement ||
            //     RuntimeData.GetEngagementObjective == EngagementObjective.None)
            // {
            //     Debug.LogWarning("Incorrect engagement objective! Current objective: " + RuntimeData.GetEngagementObjective);
            //     ForceEngagementObjective(EngagementObjective.Aggressive);
            //     Debug.LogWarning("Forced objective to: " + RuntimeData.GetEngagementObjective);
            // }

            switch (currentObj)
            {
                //! fall back to aggressive/judgement
                case EngagementObjective.None:

                case EngagementObjective.Judgement:
                    if (Brain.DebugEnabled) print("Anticipation: Aggressive/Judgement.");
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
                case EngagementObjective.Ambush:
                    if (Brain.DebugEnabled) print("Anticipation: Ambush.");
                    var mostPopulatedCavern = CavernManager.GetMostPopulatedCavern();
                    bool hasConnectedCaverns = mostPopulatedCavern != null && mostPopulatedCavern.ConnectedCaverns.IsNotEmpty();
                    targetCavern = CavernManager.GetLeastPopulatedCavern(hasConnectedCaverns);
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
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, RuntimeData.GetEngagementObjective != EngagementObjective.Judgement);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
            //NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
            Brain.UpdateNextMoveCavern(nextCavern);

            if (Brain.DebugEnabled) "Determining Next Cavern".Msg();
        }

        private bool CheckForJudgementStateCondition()
        {
            if (Brain.CurrentTarget != null)
            {
                RuntimeData.TickAnticipationTicker(Time.deltaTime);
                RuntimeData.SetBrainState(BrainState.Judgement);
                if (Brain.DebugEnabled) Debug.Log("Spotted and entered engagement!");
                return true;
            }

            return false;
        }

        private bool CheckForAmbushStateCondition()
        {
            toGoAmbushTimer -= Brain.DeltaTime;
            if (ToGoAmbushTimerReached())
            {
                ResetToGoAmbushTimer();
                RuntimeData.SetBrainState(BrainState.Ambush);
                if (Brain.DebugEnabled) Debug.Log("Took too long and VERY HANGRY, preparing to ambush!!!");
                return true;
            }

            return false;

            void ResetToGoAmbushTimer() => toGoAmbushTimer = settings.ToGoAmbushTime;
            bool ToGoAmbushTimerReached() => toGoAmbushTimer <= 0f;
        }

        void ForceEngagementObjective(EngagementObjective newObjective) => RuntimeData.SetEngagementObjective(newObjective);
        public override Func<bool> ShouldTerminate() => () => false;
    }
}