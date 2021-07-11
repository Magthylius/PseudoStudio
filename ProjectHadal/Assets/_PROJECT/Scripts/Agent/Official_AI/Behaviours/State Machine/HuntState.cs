using Tenshi;
using Tenshi.UnitySoku;
using System;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class HuntState : AIStateBase
    {
        private EngagementStateSettings settings;
        private AISenseDetection sensory;
        private CavernTag targetTag;
        private bool hasReachedTargetCavern;

        public HuntState(AIBrain brain)
        {
            Initialize(brain);
            settings = brain.MachineData.Engagement;
            sensory = brain.SenseDetection;
            ResetCachedTags();
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            RuntimeData.ResetEngagementTicker();
            targetTag = Brain.TargetMoveCavern.cavernTag;
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Hunt);
            NavigationHandler.SetSpeedMultiplier(settings.HU_RoamingSpeedMultiplier);
        }
        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickEngagementTicker(Brain.DeltaTime);
            if (RuntimeData.GetEngagementTicks > settings.HU_MaxHuntingTime)
                RuntimeData.SetBrainState(BrainState.Anticipation);
            
            if (Brain.CheckForJudgementStateCondition())
            {
                RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                return;
            }
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
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
            NavigationHandler.ResetSpeedMultiplier();
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            if (hasReachedTargetCavern)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
                return;
            }

            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, false);
            CavernTag nextTag = nextCavern.cavernTag;
            
            //! do not go through cavern linger timer, immediately go to next cavern as fast as possible
            NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
            Brain.UpdateNextMoveCavern(AICavern);
            if (nextTag == targetTag)
                hasReachedTargetCavern = true;
            
            if (Brain.DebugEnabled) $"Hunt: Determined Next Cavern to be {nextCavern.cavernTag}.".Msg();
        }

        public override void OnCavernLeave(CavernHandler cavern)
        {
            if (hasReachedTargetCavern)
            {
                DoRoar();
            }
        }

        private void ResetCachedTags()
        {
            targetTag = CavernTag.Invalid;
            hasReachedTargetCavern = false;
        }

        private void DoRoar()
        {
            AudioBank.Play3D(soundType: AISound.Roar, Brain.transform.position);
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
