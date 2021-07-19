using Tenshi;
using Tenshi.UnitySoku;
using System;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class HuntState : AIStateBase
    {
        private EngagementStateSettings settings;
        private CavernTag targetTag;
        private bool hasReachedTargetCavern;
		private float checkTimer;

        public HuntState(AIBrain brain)
        {
            Initialize(brain);
            settings = brain.MachineData.Engagement;
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
            
			if (AICavern != null && AICavern.cavernTag == Brain.TargetMoveCavern.cavernTag)
			{
				hasReachedTargetCavern = true;
			}
			else targetTag = Brain.TargetMoveCavern.cavernTag;
            
			SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Hunt);
            NavigationHandler.SetSpeedMultiplier(settings.HU_RoamingSpeedMultiplier);
			NavigationHandler.SetIgnoreCavernLingerTimer(true);
			
			checkTimer = settings.HU_PeriodicCavernUpdateTime;
        }
        public override void StateTick()
        {
            if (!AllowStateTick) return;

            RuntimeData.TickEngagementTicker(Brain.DeltaTime);
            if (RuntimeData.GetEngagementTicks > settings.HU_MaxHuntingTime)
                RuntimeData.SetBrainState(BrainState.Anticipation);
            
			checkTimer -= Brain.DeltaTime;
			if (checkTimer <= 0f)
			{
				checkTimer = settings.HU_PeriodicCavernUpdateTime;
				OnCavernEnter(AICavern);
			}
			
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
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
            NavigationHandler.ResetSpeedMultiplier();
			NavigationHandler.SetIgnoreCavernLingerTimer(false);
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
            
            //! do not go through cavern linger timer, immediately go to next cavern as fast as possible
            NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
            Brain.UpdateNextMoveCavern(nextCavern);
            if (nextTag == targetTag)
                hasReachedTargetCavern = true;
            
            if (Brain.DebugEnabled) $"Hunt: Determined Next Cavern to be {nextCavern.cavernTag}.".Msg();

             Brain.NavigationHandler.CavernModeSteering(); 
        }

        public override void OnCavernLeave(CavernHandler cavern)
        {
            if (hasReachedTargetCavern)
            {
                DoRoar();
            }
        
            Brain.NavigationHandler.TunnelModeSteering(); 
        
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
            AudioBank.Play3D(soundType: AISound.Roar, Brain.transform);
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
