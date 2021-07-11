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
        AISenseDetection sensory;
        private float autoActTimer = 0f;

        //! Meant just for startup
        private bool gameStartupInitialization = false;

        public AnticipationState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Anticipation;
            sensory = brain.SenseDetection;

            //Debug.LogWarning("heyheyinit");
            GameManager.Instance.GameStartedEvent += StartInitialization;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
            RuntimeData.ResetAnticipationTicker();
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));
            NavigationHandler.SetCanPath(true);

            autoActTimer = settings.AutoActTime;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickAnticipationTicker(Brain.DeltaTime);
            if (Brain.CheckForJudgementStateCondition()) return;
            if (CheckForAutoActCondition()) return;
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
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
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

            switch (currentObj)
            {
                case EngagementObjective.Hunt:
                    if (Brain.DebugEnabled) "Anticipation: Hunt.".Msg();
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
                    break;
                    
                case EngagementObjective.Ambush:
                    if (Brain.DebugEnabled) "Anticipation: Ambush.".Msg();
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

            Brain.UpdateTargetMoveCavern(targetCavern);
            CavernManager.SeedCavernHeuristics(targetCavern);
        }

        void DetermineNextCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, RuntimeData.GetEngagementObjective != EngagementObjective.Hunt);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
            Brain.UpdateNextMoveCavern(nextCavern);

            if (Brain.DebugEnabled) "Determining Next Cavern".Msg();
        }

        private bool CheckForAutoActCondition()
        {
            autoActTimer -= Brain.DeltaTime;
            if (AutoActTimerReached())
            {
                ResetAutoActTimer();

                RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));

                string debugMsg = string.Empty;
                if (RuntimeData.GetEngagementObjective == EngagementObjective.Ambush)
                {
                    RuntimeData.SetBrainState(BrainState.Ambush);
                    debugMsg = "Took too long and VERY HANGRY, preparing to ambush!!!";
                }
                else if (RuntimeData.GetEngagementObjective == EngagementObjective.Hunt)
                {
                    RuntimeData.SetBrainState(BrainState.Hunt);
                    SetNewTargetCavern();
                    debugMsg = "Took too long and VERY ANGRY, preparing to hunt!!!";
                }
                if (Brain.DebugEnabled) Debug.Log(debugMsg);
                return true;
            }

            return false;

            void ResetAutoActTimer() => autoActTimer = settings.AutoActTime;
            bool AutoActTimerReached() => autoActTimer <= 0f;
        }

        void ForceEngagementObjective(EngagementObjective newObjective) => RuntimeData.SetEngagementObjective(newObjective);
        public override Func<bool> ShouldTerminate() => () => false;
    }
}