using Tenshi;
using Tenshi.UnitySoku;
using System;
using Hadal.AI.Caverns;
using System.Collections;
using UnityEngine;

namespace Hadal.AI.States
{
    public class HuntState : AIStateBase
    {
        private EngagementStateSettings settings;
        private CavernHandler cachedCavern;
        private CavernTag targetTag;
        private bool hasReachedTargetCavern;

        public HuntState(AIBrain brain)
        {
            Initialize(brain);
            settings = brain.MachineData.Engagement;
            cachedCavern = null;
            ResetCachedTags();

            RuntimeData.OnCumulativeDamageCountReached += TryToTargetClosestPlayerInAICavern;
        }

        ~HuntState()
        {
            RuntimeData.OnCumulativeDamageCountReached -= TryToTargetClosestPlayerInAICavern;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            RuntimeData.ResetEngagementTicker();
            RuntimeData.ResetCumulativeDamageCount();
            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.HU_DisruptionDamageCount);
            DoRoar();

            hasReachedTargetCavern = false;
            if (AICavern != null && AICavern.cavernTag == Brain.TargetMoveCavern.cavernTag)
                hasReachedTargetCavern = true;
            
            targetTag = Brain.TargetMoveCavern.cavernTag;

            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Hunt);
            NavigationHandler.SetSpeedMultiplier(settings.HU_RoamingSpeedMultiplier);
            NavigationHandler.SetIgnoreCavernLingerTimer(true);
            // NavigationHandler.StopQueuedPath();
            TryUpdateCachedCavern();
        }
        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickEngagementTicker(Brain.DeltaTime);
            if (RuntimeData.GetEngagementTicks > settings.HU_MaxHuntingTime)
                RuntimeData.SetBrainState(BrainState.Anticipation);

            if (RuntimeData.GetEngagementTicks > settings.HU_PeriodicCavernUpdateTime)
                UpdateTargetCavern();
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
            ResetCachedTags();
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
            NavigationHandler.ResetSpeedMultiplier();
            NavigationHandler.SetIgnoreCavernLingerTimer(false);
        }

        private Coroutine cavernRoutine = null;
        public override void OnCavernEnter(CavernHandler cavern)
        {
            TryUpdateCachedCavern();
            if (hasReachedTargetCavern)
            {
                RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                RuntimeData.SetBrainState(BrainState.Anticipation);
                return;
            }

            if (cavernRoutine != null)
                Brain.StopCoroutine(cavernRoutine);

            cavernRoutine = null;
            cavernRoutine = Brain.StartCoroutine(WaitForAICavern());

            IEnumerator WaitForAICavern()
            {
                while (AICavern == null)
                    yield return null;

                CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, false);
                CavernTag nextTag = nextCavern.cavernTag;

                //! do not go through cavern linger timer, immediately go to next cavern as fast as possible
                NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
                Brain.UpdateNextMoveCavern(nextCavern);

                if (nextTag == targetTag)
                    hasReachedTargetCavern = true;

                if (Brain.DebugEnabled) $"Hunt: Determined Next Cavern to be {nextCavern.cavernTag}.".Msg();

                Brain.NavigationHandler.CavernModeSteering();
                DoRoar();
            }
        }

        public override void OnCavernLeave(CavernHandler cavern)
        {
            Brain.NavigationHandler.TunnelModeSteering();
        }

        private void UpdateTargetCavern()
        {
            CavernHandler targetCavern = CavernManager.GetMostPopulatedCavern(true);
            if (targetCavern == null) return; //! do not update if cannot find most populated cavern

            CavernTag currentTag = AICavern != null ? AICavern.cavernTag : CavernTag.Invalid;
            if (Brain.NextMoveCavern == null)
                return;
            
            if (targetCavern.cavernTag == Brain.NextMoveCavern.cavernTag)
            {
                //! AI is moving through a tunnel to the next cavern
                if (currentTag == CavernTag.Invalid)
                {
                    hasReachedTargetCavern = true;
                }
                //! AI is in a cavern
                else
                {
                    //! AI is not in next cavern
                    if (currentTag != Brain.NextMoveCavern.cavernTag)
                    {
                        hasReachedTargetCavern = true;
                    }
                    //! AI is in next cavern (which is target cavern)
                    else
                    {
                        hasReachedTargetCavern = true;
                        OnCavernEnter(AICavern);
                        return;
                    }
                }
            }

            Brain.UpdateTargetMoveCavern(targetCavern);
            CavernManager.SeedCavernHeuristics(targetCavern);
            targetTag = Brain.TargetMoveCavern.cavernTag;
        }

        private void TryToTargetClosestPlayerInAICavern()
        {
            if (RuntimeData.GetBrainState != BrainState.Hunt)
                return;
            Brain.TryToTargetClosestPlayerInAICavern();
        }

        private void ResetCachedTags()
        {
            targetTag = CavernTag.Invalid;
            hasReachedTargetCavern = false;
        }

        private void DoRoar()
        {
            int rando = UnityEngine.Random.Range(0, 2);
            if (rando == 2)
            {
                AudioBank.PlayOneShot_RoarWithDistance(Brain.transform);
            }
            
        }

        private void TryUpdateCachedCavern()
        {
            if (AICavern != null)
                cachedCavern = AICavern;
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
