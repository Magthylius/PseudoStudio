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
        EngagementStateSettings settings;
        private bool isBehaviourRunning;
        private Coroutine currentRoutine;
        private float carryDelayTimer;
        private bool canCarry;
        private bool isThreshing;

        public JudgementState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;
            ResetStateValues();
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
            Brain.RuntimeData.ResetEngagementTicker();
        }

        public override void StateTick()
        {
            float deltaTime = Brain.DeltaTime;
            RuntimeData.TickEngagementTicker(deltaTime);

            if (isBehaviourRunning)
                return;

            float ratio = HealthManager.GetHealthRatio;
            int playerCount = CavernManager.GetHandlerOfAILocation.GetPlayerCount;

            //! Defensive (?)
            if (ratio > settings.HealthRatioThreshold)
            {
                PerformBehaviour(true, playerCount);
                return;
            }

            //! Aggressive (?)
            PerformBehaviour(false, playerCount);
        }
        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd()
        {
            //Debug.LogWarning("Judgement Exit!");
            //! Subject to change
            /*if (Brain.CarriedPlayer != null)
            {
                Brain.CarriedPlayer.SetIsCarried(false);
                Brain.CarriedPlayer = null;
                Brain.AttachCarriedPlayerToMouth(false);
                Brain.NavigationHandler.StopCustomPath(false);
            }*/
            
            Brain.StopCoroutine(currentRoutine);
            currentRoutine = null;
            Brain.NavigationHandler.StopCustomPath(false);
        }

        private void PerformBehaviour(bool isDefensive, int playerCount)
        {
            switch (playerCount)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                default:
                    break;
            }
        }

        private IEnumerator DefensiveStance1()
        {
            isBehaviourRunning = true;
            bool waitForJtimer = false;

            while (isBehaviourRunning)
            {
                if (PlayerCountDropsTo0) //! Stop when there are no more players
                    break;

                if (CarryDelayTimerReached && canCarry) //! Carry if delay timer is reached & is allowed to carry
                {
                    canCarry = false;
                    Brain.TryCarryTargetPlayer();
                }

                if (Brain.IsCarryingAPlayer() && !isThreshing) //! Thresh if is carrying a player & is not yet threshing
                {
                    isThreshing = true;
                    DamageManager.ApplyDoT(Brain.CarriedPlayer, settings.G_TotalThreshTimeInSeconds, settings.G_ThreshDamagePerSecond);
                    continue;
                }
                
                if (Brain.CurrentTarget == null) //! Wait for next frame if there are no targets detected nearby
                    yield return null;
                
                SetCustomNavPoint(Brain.CurrentTarget); //! Set custom nav point to destination: current target player if not already moving towards it

                if (CloseThresholdReached && !canCarry) //! Start delay timer if close enough to player & is waiting to be allowed to carry
                {
                    canCarry = true;
                    SetCarryDelayTimer();
                    continue;
                }
                
                if (FarThresholdReached) //! Stop behaviour and wait for Jtimer if too far away from current target player
                {
                    waitForJtimer = true;
                    break;
                }

                yield return null;
            }

            while (!IsJudgementThresholdReached(4) && waitForJtimer)
                yield return null;
            
            //! Handle Behaviour ending
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            yield return null;
        }

        #region Utility Methods

        private void SetCustomNavPoint(PlayerController player)
        {
            NavPoint point = player.GetComponentInChildren<NavPoint>();
            if (point == null)
            {
                point = Object.Instantiate(RuntimeData.navPointPrefab, player.GetTarget.position, Quaternion.identity);
                point.tag = "NavigationPoint";
                point.AttachTo(player.transform);
                point.SetCavernTag(Caverns.CavernTag.Custom_Point);
                NavigationHandler.SetCustomPath(point, true);
            }
        }

        private void ResetStateValues()
        {
            isBehaviourRunning = false;
            currentRoutine = null;
            carryDelayTimer = 0f;
            canCarry = false;
            isThreshing = false;
        }

        #endregion

        #region Verbose Shorthands

        private bool PlayerCountDropsTo0
            => CavernManager.GetHandlerOfAILocation.GetPlayerCount == 0;

        private float SqrDistanceToTarget
            => (Brain.CurrentTarget.GetTarget.position - Brain.transform.position).sqrMagnitude;

        private bool CloseThresholdReached
            => SqrDistanceToTarget < settings.G_ApproachCloseDistanceThreshold.Sqr();
        
        private bool FarThresholdReached
            => SqrDistanceToTarget > settings.G_ApproachFarDistanceThreshold.Sqr();

        private bool IsJudgementThresholdReached(int i)
            => RuntimeData.HasJudgementTimerOfIndexExceeded(i);

        private void SetCarryDelayTimer() => carryDelayTimer = Time.time + settings.G_CarryDelayTimer;
        private bool CarryDelayTimerReached => Time.time > carryDelayTimer;

        #endregion
        
        public override Func<bool> ShouldTerminate() => () => false;
    }
}
