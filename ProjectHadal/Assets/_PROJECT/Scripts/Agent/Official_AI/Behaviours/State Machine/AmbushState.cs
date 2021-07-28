using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;
using UnityEngine;
using System.Collections;

namespace Hadal.AI.States
{
    public class AmbushState : AIStateBase
    {
        EngagementStateSettings settings;
        float ambushTimer;
        bool playAmbushCloseAudioOnce = true;

        public AmbushState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;

        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            NavigationHandler.OnReachedAmbushPoint += HandlePointReachedEvent;
            RuntimeData.ResetEngagementTicker();
            RuntimeData.ResetCumulativeDamageCount();
            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.AM_DisruptionDamageCount);
            ambushTimer = settings.AM_MaxWaitTime;
            SenseDetection.SetDetectionMode(AISenseDetection.DetectionMode.Ambush);
            NavigationHandler.StopQueuedPath();
            SelectNewAmbushPoint();
            playAmbushCloseAudioOnce = true;

        }
        public override void StateTick()
        {
            if (DetectIfCoverCompromised()) return;
            if (CheckPouncingRange()) return;
            if (CheckAmbushTimer()) return;
        }

        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd()
        {
            NavigationHandler.OnReachedAmbushPoint -= HandlePointReachedEvent;
            NavigationHandler.ResetAmbushPoint();
            AnimationManager.ResetSpeed();
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

        void SelectNewAmbushPoint()
        {
            if (!NavigationHandler.Data_ChosenAmbushPoint)
            {
                NavigationHandler.SelectAmbushPoint();
                AudioBank.PlayOneShot(soundType: AISound.Ambush, Brain.transform);
            }
        }

        bool DetectIfCoverCompromised()
        {
            if (RuntimeData.IsCumulativeDamageCountReached)
            {
                RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                RuntimeData.SetBrainState(BrainState.Recovery);
                return true;
            }
            return false;
        }


  
        /// <summary>
        /// Detect players and if in range, pounce, else go to recovery. 
        /// </summary>
        bool CheckPouncingRange()
        {
            if (AICavern != null)
            {
                if (AICavern.GetPlayerCount > 0 && playAmbushCloseAudioOnce)
                {
                    Debug.LogWarning("YO");
                    Brain.ambiencePlayer.PlayAmbienceOfType(AudioSystem.AmbienceType.AmbushHeartbeat);
                    playAmbushCloseAudioOnce = false;
                }
            }

            if (SenseDetection.DetectedPlayersCount > 0 && SenseDetection.DetectedPlayersCount < 4)
            {
                //! wait for sense detection to handle current target
                if (Brain.CurrentTarget != null)
                {
                   
                    RuntimeData.UpdateConfidenceValue(settings.ConfidenceIncrementValue);
                    RuntimeData.SetBrainState(BrainState.Judgement);
                    return true;
                }
            }
            else if (SenseDetection.DetectedPlayersCount == 4)
            {
                RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                RuntimeData.SetBrainState(BrainState.Recovery);
                return true;
            }

            return false;
        }

        bool CheckAmbushTimer()
        {
            ambushTimer -= Brain.DeltaTime;

            if (ambushTimer < 0)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
                return true;
            }
            return false;
        }

        private void HandlePointReachedEvent()
        {
            if (RuntimeData.GetBrainState != BrainState.Ambush)
                return;

            AnimationManager.SetSpeed(0.1f);
        }
    }
}
