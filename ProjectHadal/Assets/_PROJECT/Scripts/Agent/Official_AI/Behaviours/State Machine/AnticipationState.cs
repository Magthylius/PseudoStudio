using System;
using System.Collections;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;
using Hadal.Player;

//! C: jet, E: jon
namespace Hadal.AI.States
{
    public class AnticipationState : AIStateBase
    {
        AnticipationStateSettings settings;
        private float autoActTimer = 0f;
        private float isolatedPlayerCheckTimer = 0f;
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
            isolatedPlayerCheckTimer = settings.IsolatedPlayerCheckTime;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickAnticipationTicker(Brain.DeltaTime);
            
            if (CheckForIsolatedPlayerCondition(Brain.DeltaTime)) return;
            if (Brain.CheckForJudgementStateCondition()) return;
            if (CheckForAutoActCondition(Brain.DeltaTime)) return;
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

            Brain.NavigationHandler.CavernModeSteering(); 
        }

        public override void OnCavernLeave(CavernHandler cavern)
        {
            Brain.NavigationHandler.TunnelModeSteering(); 
        }

        void StartInitialization(bool booleanData)
        {
            GameManager.Instance.StartCoroutine(InitializeAfterCaverns());
        }

        IEnumerator InitializeAfterCaverns()
        {
			while (Brain == null)
			{
				yield return null;
			}

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
                yield return null;
            }

            while (AICavern == null)
            {
                yield return null;
            }

            AllowStateTick = true;
            RuntimeData.SetEngagementObjective(settings.GetRandomInfluencedObjective(RuntimeData.NormalisedConfidence));

            gameStartupInitialization = true;
            DetermineNextCavern();
        }

        CavernHandler SetNewTargetCavern()
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
            return targetCavern;
        }
        void DetermineNextCavern()
        {
            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, RuntimeData.GetEngagementObjective != EngagementObjective.Hunt);
            NavigationHandler.ComputeCachedDestinationCavernPath(nextCavern);
            NavigationHandler.EnableCachedQueuePathTimer();
            Brain.UpdateNextMoveCavern(nextCavern);

            if (Brain.DebugEnabled) "Determining Next Cavern".Msg();
        }

        private bool CheckForIsolatedPlayerCondition(in float deltaTime)
        {
            isolatedPlayerCheckTimer -= deltaTime;
            if (IsolatedPlayerCheckTimerReached())
            {
                //! Reset timer
                ResetIsolatedPlayerCheckTimer();

                //! Get any available isolated player, if none is found exit immediately with false
                PlayerController isolatedPlayer = GetAnyIsolatedPlayer();
                if (isolatedPlayer == null)
                    return false;
                
                //! Try to set current target of the brain & return the result of the judgement check
                Brain.TrySetCurrentTarget(isolatedPlayer);
                return Brain.CheckForJudgementStateCondition();
            }

            return false;

            void ResetIsolatedPlayerCheckTimer() => isolatedPlayerCheckTimer = settings.IsolatedPlayerCheckTime;
            bool IsolatedPlayerCheckTimerReached() => isolatedPlayerCheckTimer <= 0f;
            PlayerController GetAnyIsolatedPlayer()
            {
                var player = AICavern != null ? AICavern.GetIsolatedPlayer() : null;
                if (player == null) player = SenseDetection.GetIsolatedPlayerIfAny();
                return player;
            }
        }

        private bool CheckForAutoActCondition(in float deltaTime)
        {
            autoActTimer -= deltaTime;
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
                        CavernHandler targetCavern = null;
                        CavernTag aiCavernTag = AICavern != null ? AICavern.cavernTag : CavernTag.Invalid;
                        if (anyPlayersToHunt)
                        {
                            targetCavern = SetNewTargetCavern();
                            debugMsg = "Took too long and VERY ANGRY, preparing to hunt!!!";
                        }
                        
                        if (!anyPlayersToHunt || targetCavern.cavernTag == aiCavernTag)
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