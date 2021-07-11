using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;
using UnityEngine;

namespace Hadal.AI.States
{
    public class AmbushState : AIStateBase
    {
        EngagementStateSettings settings;
        AISenseDetection sensory;
        CavernHandler cavernHandler;
        CavernTag currentCavern;
        float ambushTimer;
        float ambushDetectionRadius;

        public AmbushState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;
            sensory = brain.SenseDetection;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            currentCavern = Brain.CavernManager.GetCavernTagOfAILocation();
            cavernHandler = Brain.CavernManager.GetCavern(currentCavern);
            ambushTimer = settings.AM_MaxWaitTime;
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Ambush);
        }
        public override void StateTick()
        {

            SelectNewAmbushPoint();
            CheckPouncingRange();
            CheckAmbushTimer();

        }


        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd()
        {
            NavigationHandler.ResetAmbushPoint();
            sensory.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
        }
        public override Func<bool> ShouldTerminate() => () => false;

        void CheckPlayerCountAtNeighbourCaverns()
        {
            
        }


        void SelectNewAmbushPoint()
        {
            if (!NavigationHandler.Data_ChosenAmbushPoint)
            {
                NavigationHandler.SelectAmbushPoint();
            }
        }

        /// <summary>
        /// Detect players and if in range, pounce, else go to recovery. 
        /// </summary>
        void CheckPouncingRange()
        {
            if (sensory.DetectedPlayersCount > 0 && sensory.DetectedPlayersCount < 4)
            {
                float distance = Vector3.Distance(Brain.transform.position, Brain.CurrentTarget.transform.position);
                if (distance < settings.AM_TargetPlayerRange && Brain.CurrentTarget != null)
                {
                    RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                    RuntimeData.SetBrainState(BrainState.Judgement);
                }
                else
                {
                    RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                    RuntimeData.SetBrainState(BrainState.Recovery);
                }

            }
            else if (sensory.DetectedPlayersCount == 4 && Brain.CurrentTarget != null)
            {
                RuntimeData.UpdateConfidenceValue(settings.ConfidenceDecrementValue);
                RuntimeData.SetBrainState(BrainState.Recovery);
            }
        }

        void CheckAmbushTimer()
        {
            ambushTimer -= Brain.DeltaTime;

            if (ambushTimer < 0)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
            }
        }
    }
}
