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
        private CavernTag currentTag;
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

            currentTag = AICavern.cavernTag;
            targetTag = Brain.TargetMoveCavern.cavernTag;
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Hunt);
            NavigationHandler.SetSpeedMultiplier(settings.HU_RoamingSpeedMultiplier);
        }
        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickEngagementTicker(Brain.DeltaTime);
            if (Brain.CheckForJudgementStateCondition()) return;
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

            CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true);
            CavernTag nextTag = nextCavern.cavernTag;
            
            if (nextTag != currentTag)
            {
                currentTag = nextTag;
                if (nextTag == targetTag)
                    hasReachedTargetCavern = true;

                //! do not go through cavern linger timer, immediately go to next cavern as fast as possible
                NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
                Brain.UpdateTargetMoveCavern(AICavern);
            }

            if (Brain.DebugEnabled) $"Hunt: Determined Next Cavern to be {nextCavern.cavernTag}.".Msg();
        }

        private void ResetCachedTags()
        {
            currentTag = CavernTag.Invalid;
            targetTag = CavernTag.Invalid;
            hasReachedTargetCavern = false;
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
