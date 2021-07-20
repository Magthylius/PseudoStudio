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
        CavernHandler cavernHandler;
        CavernTag currentCavern;
        float ambushTimer;
        float ambushDetectionRadius;

        public AmbushState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            SelectNewAmbushPoint();
            RuntimeData.ResetEngagementTicker();
            RuntimeData.ResetCumulativeDamageCount();
            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.AM_DisruptionDamageCount);
            currentCavern = Brain.CavernManager.GetCavernTagOfAILocation();
            cavernHandler = Brain.CavernManager.GetCavern(currentCavern);
            ambushTimer = settings.AM_MaxWaitTime;
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Ambush);
            NavigationHandler.StopQueuedPath();
        }
        public override void StateTick()
        {

            DetectIfCoverCompromised();
            CheckPouncingRange();
            CheckAmbushTimer();

        }


        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd()
        {
            NavigationHandler.ResetAmbushPoint();
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Normal);
        }
        public override void OnCavernEnter(CavernHandler cavern)
        {
            Brain.NavigationHandler.CavernModeSteering();
        }
        public override void OnCavernLeave(CavernHandler cavern)
        {
            Brain.NavigationHandler.TunnelModeSteering();
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

        void DetectIfCoverCompromised()
        {
            if (RuntimeData.IsCumulativeDamageCountReached)
            {
                RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                RuntimeData.SetBrainState(BrainState.Recovery);
            }
        }

        /// <summary>
        /// Detect players and if in range, pounce, else go to recovery. 
        /// </summary>
        void CheckPouncingRange()
        {
            if (SenseDetection.DetectedPlayersCount > 0 && SenseDetection.DetectedPlayersCount < 4)
            {
                //1 commenting this out because Brain.CurrentTarget.transform.position will give NullReferenceException if Brain.CurrentTarget == null
                // float distance = Vector3.Distance(Brain.transform.position, Brain.CurrentTarget.transform.position);
                // if (distance < settings.AM_TargetPlayerRange && Brain.CurrentTarget != null)
                // {
                //     RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                //     RuntimeData.SetBrainState(BrainState.Judgement);
                // }
                // else
                // {
                //     //1 commenting this because it makes the AI go to recovery before it can ambush
                //     // RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                //     // RuntimeData.SetBrainState(BrainState.Recovery);
                // }

                //! wait for sense detection to handle current target
                if (Brain.CurrentTarget != null)
                {
                    RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                    RuntimeData.SetBrainState(BrainState.Judgement);
                }
            }
            else if (SenseDetection.DetectedPlayersCount == 4)
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
