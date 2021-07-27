using System;
using Tenshi.UnitySoku;
using Tenshi;
using UnityEngine;
using Hadal.Player;
using System.Linq;

namespace Hadal.AI.States
{
    public class JudgementState : AIStateBase
    {
        private EngagementStateSettings settings;
        private JudgementBehaviourCoroutines behaviour;
        private Coroutine currentRoutine;
        public bool IsBehaviourRunning { get; set; }
        public bool ShouldExit { get; set; }
        public PlayerController IsolatedPlayer { get; private set; }
        
        public JudgementState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;
            behaviour = new JudgementBehaviourCoroutines(brain, this);
            ResetStateValues();
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            AllowStateTick = true;
            ShouldExit = false;
			
			Brain.Send_JudgementEvent(true);
            RuntimeData.ResetEngagementTicker();
            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.G_DisruptionDamageCount);
			AudioBank.PlayOneShot(AISound.GrabRiser, Brain.transform);

            if (RuntimeData.IsPreviousBrainStateEqualTo(BrainState.Ambush))
            {
                AllowStateTick = false;
                NavigationHandler.SetSpeedMultiplier(settings.AM_PounceSpeedMultiplier);
                RuntimeData.ResetCumulativeDamageCount();
                PerformAmbushLinkBehaviour();
            }
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (ShouldExit)
            {
                RuntimeData.SetBrainState(BrainState.Recovery);
                return;
            }

            if (IsBehaviourRunning || !AllowStateTick)
                return;

            int playerCount;
            var handler = CavernManager.GetHandlerOfAILocation;
            if (handler != null)
            {
                playerCount = handler.GetPlayerCount;
                IsolatedPlayer = handler.GetIsolatedPlayer();
            }
            else
            {
                SenseDetection.RequestImmediateSensing();
                playerCount = SenseDetection.DetectedPlayersCount;
                IsolatedPlayer = SenseDetection.GetIsolatedPlayerIfAny(false);
            }

            //! Cannot target down or unalive players again
            if (IsolatedPlayer != null && IsolatedPlayer.GetInfo.HealthManager.IsDownOrUnalive)
                IsolatedPlayer = null;

            RuntimeData.ResetEngagementTicker();
            RuntimeData.ResetCumulativeDamageCount();

            bool isDefensive = RuntimeData.NormalisedConfidence < 0.5f;
            PerformBehaviour(isDefensive, playerCount);
        }
        public override void LateStateTick() { if (AllowStateTick) return; }
        public override void FixedStateTick() { if (AllowStateTick) return; }
        public override void OnStateEnd()
        {
            StopAnyRunningCoroutines();
            if (behaviour != null) behaviour.ResetJudgementBehaviour();
            
			Brain.Send_JudgementEvent(false);
			Brain.DetachAnyCarriedPlayer();
            NavigationHandler.ResetSpeedMultiplier();
            NavigationHandler.StopCustomPath(false);
			NavigationHandler.SetLookAtTarget(null);
            AnimationManager.SetAnimation(AIAnim.Swim);
        }

        /// <summary> Performs a behaviour based on the boolean. If not defensive, it will be aggressive. </summary>
        private void PerformBehaviour(bool isDefensive, int playerCount)
        {
            StopAnyRunningCoroutines();
            if (isDefensive)
                currentRoutine = Brain.StartCoroutine(behaviour.DefensiveStance(playerCount));
            else
                currentRoutine = Brain.StartCoroutine(behaviour.AggressiveStance(playerCount));
        }

        private void PerformAmbushLinkBehaviour()
        {

            StopAnyRunningCoroutines();
            //Debug.LogError("LINK");
            currentRoutine = Brain.StartCoroutine(behaviour.AmbushStance());
        }

        public void ResetStateValues()
        {
            IsBehaviourRunning = false;
            IsolatedPlayer = null;
        }

        public void StopAnyRunningCoroutines()
        {
            if (currentRoutine == null) return;
            Brain.StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
        
        public override Func<bool> ShouldTerminate() => () => false;
    }
}
