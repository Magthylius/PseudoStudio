using System;
using System.Collections;
using Tenshi.UnitySoku;
using Tenshi;
using UnityEngine;
using Hadal.Player;
using Object = UnityEngine.Object;

namespace Hadal.AI.States
{
    public class JudgementState : AIStateBase
    {
        private EngagementStateSettings settings;
        private JudgementBehaviourCoroutines behaviour;
        private Coroutine currentRoutine;
        public bool IsBehaviourRunning { get; set; }
        
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
            RuntimeData.ResetEngagementTicker();

            if (RuntimeData.IsPreviousBrainStateEqualTo(BrainState.Ambush))
            {
                AllowStateTick = false;
                NavigationHandler.SetSpeedMultiplier(settings.AM_PounceSpeedMultiplier);
                PerformAmbushLinkBehaviour();
            }
        }

        public override void StateTick()
        {
            float deltaTime = Brain.DeltaTime;
            RuntimeData.TickEngagementTicker(deltaTime);

            if (IsBehaviourRunning || !AllowStateTick)
                return;

            int playerCount = CavernManager.GetHandlerOfAILocation.GetPlayerCount;

            //! Defensive (?)
            if (HealthManager.GetHealthRatio > settings.HealthRatioThreshold)
            {
                PerformBehaviour(true, playerCount);
                return;
            }

            //! Aggressive (?)
            PerformBehaviour(false, playerCount);
        }
        public override void LateStateTick() { if (AllowStateTick) return; }
        public override void FixedStateTick() { if (AllowStateTick) return; }
        public override void OnStateEnd()
        {
            StopAnyRunningCoroutines();
            if (behaviour != null) behaviour.ResetStateValues();
            NavigationHandler.ResetSpeedMultiplier();
            NavigationHandler.StopCustomPath(true);
        }

        private void PerformBehaviour(bool isDefensive, int playerCount)
        {
            StopAnyRunningCoroutines();

            //! When the aggressive routine has finished implementation, will uncomment the boolean
            // if (isDefensive)
                currentRoutine = Brain.StartCoroutine(behaviour.DefensiveStance(playerCount));
        }

        private void PerformAmbushLinkBehaviour()
        {
            StopAnyRunningCoroutines();
            currentRoutine = Brain.StartCoroutine(behaviour.AmbushStance());
        }

        public void ResetStateValues()
        {
            IsBehaviourRunning = false;
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
