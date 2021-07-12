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
        private float autoActTimer = 0f;
        private bool isFirstAct = false;

        //! Meant just for startup
        private bool gameStartupInitialization = false;

        public AnticipationState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Anticipation;

            GameManager.Instance.GameStartedEvent += StartInitialization;
            RuntimeData.OnCumulativeDamageCountReached += TryToTargetClosestPlayerInAICavern;
        }

        ~AnticipationState()
        {
            RuntimeData.OnCumulativeDamageCountReached -= TryToTargetClosestPlayerInAICavern;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
            RuntimeData.ResetAnticipationTicker();
            RuntimeData.ResetCumulativeDamageCount();
            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.DisruptionDamageCount);
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));

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
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
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
                    targetCavern = CavernManager.GetMostPopulatedCavern(false);

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
                Brain.AudioBank.Play3D(AISound.Swim, Brain.transform);
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
                    if (!isFirstAct)
                    {
                        CavernHandler target = CavernManager.GetCavern(CavernTag.Crystal);
                        Brain.UpdateTargetMoveCavern(target);
                        CavernManager.SeedCavernHeuristics(target);
                        debugMsg = "Took too long and VERY ANGRY, preparing to hunt (first action = targeting Crystal cavern)!!!";
                    }
                    else
                    {
                        bool anyPlayersToHunt = CavernManager.AnyPlayersPresentInAnyCavern();
                        bool currentCavernIsMostPopulated = AICavern != null && AICavern.cavernTag == CavernManager.GetMostPopulatedCavern(false).cavernTag;
                        if (anyPlayersToHunt && !currentCavernIsMostPopulated)
                        {
                            SetNewTargetCavern();
                            debugMsg = "Took too long and VERY ANGRY, preparing to hunt!!!";
                        }
                        else
                        {
                            RuntimeData.SetBrainState(BrainState.Ambush);
                            debugMsg = "Took too long and VERY ANGRY, but hunting is not a suitable option... therefore, preparing to ambush!!!";
                        }
                    }


                }
                if (Brain.DebugEnabled) Debug.Log(debugMsg);

                isFirstAct = true;
                return true;
            }

            return false;

            void ResetAutoActTimer() => autoActTimer = settings.AutoActTime;
            bool AutoActTimerReached() => autoActTimer <= 0f;
        }

        private void TryToTargetClosestPlayerInAICavern()
        {
            if (RuntimeData.GetBrainState != BrainState.Anticipation)
                return;
            Brain.TryToTargetClosestPlayerInAICavern();
        }

        void ForceEngagementObjective(EngagementObjective newObjective) => RuntimeData.SetEngagementObjective(newObjective);
        public override Func<bool> ShouldTerminate() => () => false;
    }
}