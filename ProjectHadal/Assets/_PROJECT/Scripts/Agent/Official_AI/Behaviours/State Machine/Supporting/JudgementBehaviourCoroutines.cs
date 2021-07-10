using System.Collections;
using Hadal.AI.Caverns;
using Hadal.AI.States;
using Hadal.Player;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class JudgementBehaviourCoroutines
    {
        private AIBrain Brain;
        private EngagementStateSettings Settings;
        private PointNavigationHandler NavigationHandler;
        private LeviathanRuntimeData RuntimeData;
        private StateMachineData MachineData;
        private CavernManager CavernManager;
        private AIDamageManager DamageManager;
        private AIHealthManager HealthManager;
        private JudgementState JState;

        public JudgementBehaviourCoroutines(AIBrain brain, JudgementState judgementState)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
            RuntimeData = Brain.RuntimeData;
            MachineData = Brain.MachineData;
            Settings = MachineData.Engagement;
            CavernManager = Brain.CavernManager;
            DamageManager = Brain.DamageManager;
            HealthManager = Brain.HealthManager;
            JState = judgementState;

            ResetStateValues();

            Brain.OnStunnedEvent += HandleStunEvent;
        }

        ~JudgementBehaviourCoroutines()
        {
            Brain.OnStunnedEvent -= HandleStunEvent;
        }

        private float carryDelayTimer;
        private bool canCarry;
        private bool isAttacking;
        private bool isDamaging;
        private bool waitForJtimer;
        private CoroutineData threshRoutineData;
        private CoroutineData approachRoutineData;

        #region Coroutines

        private IEnumerator MoveToCurrentTarget(float carryDelayTime)
        {
            while (JState.IsBehaviourRunning)
            {
                //! Set custom nav point to destination: current target player if not already moving towards it.
                if (Brain.CurrentTarget != null)
                {
                    bool success = TrySetCustomNavPoint(Brain.CurrentTarget);
                    if (success) TryDebug("Set custom nav point onto target. Moving to chase target.");
                }

                //! Start delay timer if close enough to player & is waiting to be allowed to carry
                if (CloseThresholdReached && !canCarry)
                {
                    canCarry = true;
                    SetCarryDelayTimer(carryDelayTime);
                    TryDebug("Target is close enough to be Grabbed, starting delay timer before player is grabbed.");
                    continue;
                }

                //! Stop behaviour and wait for Jtimer if too far away from current target player
                if (FarThresholdReached)
                {
                    waitForJtimer = true;
                    TryDebug("Target got too far from the Leviathan, stopping behaviour.");
                    break;
                }

                //! Carry if delay timer is reached & is allowed to carry
                if (CarryDelayTimerReached && canCarry)
                {
                    canCarry = false;
                    if (!CloseThresholdReached)
                    {
                        TryDebug("Target was able to get away from being grabbed. Stopping behaviour.");
                        break;
                    }

                    bool success = Brain.TryCarryTargetPlayer();
                    TryDebug(success ? "Succeeded in carrying target player." : "Failed to carry target player, stopping behaviour.");
                    if (!success) waitForJtimer = true;

                    break;
                }

                yield return null;
            }
        }

        private IEnumerator DoThreshAttack(int dps, System.Action successCallback, System.Action failureCallback)
        {
            bool success = true;
            int totalDamageSeconds = Settings.G_TotalThreshTimeInSeconds;
            isDamaging = true;
            NavigationHandler.DisableWithLerp(2f);

            void StopAttack() => isDamaging = false;

            DamageManager.ApplyDoT(Brain.CarriedPlayer,
                totalDamageSeconds,
                dps.Abs(),
                StopAttack);

            while (isDamaging && DamageManager != null && JState.IsBehaviourRunning)
            {
                if (RuntimeData.IsCumulativeDamageCountReached)
                {
                    RuntimeData.ResetCumulativeDamageCount();
                    success = false;
                    TryDebug("Cumulative damage count threshold reached. Ending thresh damage early.");
                    break;
                }
                yield return null;
            }

            NavigationHandler.Enable();
            TryDebug("Thresh damage in inner routine is finished!");

            if (success)
                successCallback?.Invoke();
            else
                failureCallback?.Invoke();
        }

        public IEnumerator DefensiveStance(int stanceIndex)
        {
            TryDebug($"Starting Defensive Behaviour for player count: {stanceIndex}.");
            JState.IsBehaviourRunning = true;
            waitForJtimer = false;
            int jTimerIndex = 5 - stanceIndex;

            var approachRoutineData = new CoroutineData(Brain, MoveToCurrentTarget(Settings.G_CarryDelayTimer));

            while (JState.IsBehaviourRunning)
            {
                //! When the behaviour takes too long
                if (IsJudgementThresholdReached(jTimerIndex))
                {
                    TryDebug("Defensive behaviour took too long, ending immediately.");
                    break;
                }

                //! Stop when there are no more players
                if (PlayerCountDroppedTo0)
                {
                    TryDebug("No more players detected in cavern, stopping defensive behaviour.");
                    break;
                }

                //! Thresh if is carrying a player & is not yet threshing
                if (Brain.IsCarryingAPlayer() && !isAttacking)
                {
                    isAttacking = true;

                    TryDebug("Starting threshing routine.");
                    var threshRoutineData = new CoroutineData(Brain,
                        DoThreshAttack(
                            Settings.GetThreshDamagePerSecond(EngagementType.Defensive, RuntimeData.IsEggDestroyed),
                            null, null));
                    yield return threshRoutineData.Coroutine; //this will wait for the DoThreshAttack() coroutine to finish

                    TryDebug("Thresh damage in outer routine is finished!");

                    Brain.TryDropCarriedPlayer();
                    TryDebug("Attacking is done, dropping carried player. Stopping behaviour.");
                    break;
                }

                yield return null;
            }

            //! If the wait boolean is true, will wait until the judgement timer is exceeded before handling behaviour end
            while (!IsJudgementThresholdReached(jTimerIndex) && waitForJtimer)
                yield return null;

            //! Handle Behaviour ending
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            
            yield return approachRoutineData.Coroutine;
        }

        public IEnumerator AggressiveStance(int stanceIndex)
        {
            TryDebug($"Starting Aggressive Behaviour for player count: {stanceIndex}.");
            JState.IsBehaviourRunning = true;
            bool waitForJtimer = false;
            int jTimerIndex = 5 - stanceIndex;

            var approachRoutineData = new CoroutineData(Brain, MoveToCurrentTarget(Settings.G_CarryDelayTimer));

            while (JState.IsBehaviourRunning)
            {
                //! When the behaviour takes too long
                if (IsJudgementThresholdReached(jTimerIndex))
                {
                    TryDebug("Aggressive behaviour took too long, ending immediately.");
                    break;
                }

                //! Stop when there are no more players
                if (PlayerCountDroppedTo0)
                {
                    TryDebug("No more players detected in cavern, stopping aggressive behaviour.");
                    break;
                }

                //! Thresh if is carrying a player & is not yet threshing
                if (Brain.IsCarryingAPlayer() && !isAttacking)
                {
                    isAttacking = true;

                    TryDebug("Starting threshing routine.");
                    var threshRoutineData = new CoroutineData(Brain,
                        DoThreshAttack(
                            Settings.GetThreshDamagePerSecond(EngagementType.Aggressive, RuntimeData.IsEggDestroyed),
                            () => RuntimeData.UpdateConfidenceValue(Settings.ConfidenceIncrementValue),
                            null
                        )
                    );
                    yield return threshRoutineData.Coroutine; //this will wait for the DoThreshAttack() coroutine to finish

                    TryDebug("Thresh damage in outer routine is finished!");

                    Brain.TryDropCarriedPlayer();
                    TryDebug("Attacking is done, dropping carried player. Stopping behaviour.");
                    break;
                }
                yield return null;
            }

            //! If the wait boolean is true, will wait until the judgement timer is exceeded before handling behaviour end
            while (!IsJudgementThresholdReached(jTimerIndex) && waitForJtimer)
                yield return null;

            //! Handle Behaviour ending
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);

            yield return approachRoutineData.Coroutine;
        }

        public IEnumerator AmbushStance()
        {
            TryDebug($"Starting Ambush Behaviour in Judgement behaviour.");
            JState.IsBehaviourRunning = true;
            waitForJtimer = false;
            int jTimerIndex = 1;

            approachRoutineData = new CoroutineData(Brain, MoveToCurrentTarget(Settings.AM_CarryDelayTimer));

            while (JState.IsBehaviourRunning)
            {
                //! When the behaviour takes too long
                if (IsJudgementThresholdReached(jTimerIndex))
                {
                    TryDebug("Ambush behaviour took too long, ending immediately.");
                    break;
                }

                //! Thresh if is carrying a player & is not yet threshing
                if (Brain.IsCarryingAPlayer() && !isAttacking)
                {
                    isAttacking = true;

                    TryDebug("Starting threshing routine.");
                    threshRoutineData = new CoroutineData(Brain,
                        DoThreshAttack(
                            Settings.GetThreshDamagePerSecond(EngagementType.Ambushing, RuntimeData.IsEggDestroyed),
                            () => RuntimeData.UpdateConfidenceValue(Settings.ConfidenceIncrementValue),
                            () => RuntimeData.UpdateConfidenceValue(-Settings.ConfidenceDecrementValue)
                        )
                    );
                    yield return threshRoutineData.Coroutine; //this will wait for the DoThreshAttack() coroutine to finish
                    threshRoutineData.Stop();
                    threshRoutineData = null;

                    TryDebug("Thresh damage in outer routine is finished!");

                    Brain.TryDropCarriedPlayer();
                    TryDebug("Attacking is done, dropping carried player. Stopping behaviour.");
                    break;
                }

                yield return null;
            }

            //! If the wait boolean is true, will wait until the judgement timer is exceeded before handling behaviour end
            while (!IsJudgementThresholdReached(jTimerIndex) && waitForJtimer)
                yield return null;

            //! Handle Behaviour ending
            approachRoutineData.Stop();
            approachRoutineData = null;
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            
            yield return null;
        }

        #endregion

        #region Utility & Verbose Shorthands

        private bool TrySetCustomNavPoint(PlayerController player)
        {
            if (player.GetIsTaggedByLeviathan)
                return false;

            NavPoint point = Object.Instantiate(RuntimeData.navPointPrefab.gameObject, player.GetTarget.position, Quaternion.identity).GetComponent<NavPoint>();
            point.tag = "NavigationPoint";
            point.AttachTo(player.transform);
            point.SetCavernTag(CavernTag.Custom_Point);
            NavigationHandler.SetCustomPath(point, true);
            player.SetIsTaggedByLeviathan(true);
            return true;
        }

        private void HandleStunEvent(bool isStunned)
        {
            if (!isStunned)
                return;

            threshRoutineData?.Stop();
            threshRoutineData = null;
            approachRoutineData?.Stop();
            approachRoutineData = null;
            JState.IsBehaviourRunning = false;
            JState.StopAnyRunningCoroutines();
            NavigationHandler.Enable();
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Judgement);
            TryDebug("The Leviathan has been stunned. Stopping behaviour but not exiting Judgement state.");
        }

        public void ResetStateValues()
        {
            carryDelayTimer = 0f;
            canCarry = false;
            isAttacking = false;
            isDamaging = false;
            JState.ResetStateValues();
        }

        private void TryDebug(object msg)
        {
            if (Brain.DebugEnabled)
            {
                msg = "Judgement: " + msg;
                msg.Msg();
            }
        }

        private bool PlayerCountDroppedTo0
            => CavernManager.GetHandlerOfAILocation != null
            && CavernManager.GetHandlerOfAILocation.GetPlayerCount == 0;

        private float SqrDistanceToTarget
            => (Brain.CurrentTarget.GetTarget.position - Brain.transform.position).sqrMagnitude;

        private bool CloseThresholdReached
            => Brain.CurrentTarget != null
            && SqrDistanceToTarget < Settings.G_ApproachCloseDistanceThreshold.Sqr();

        private bool FarThresholdReached
            => Brain.CurrentTarget != null
            && SqrDistanceToTarget > Settings.G_ApproachFarDistanceThreshold.Sqr();

        private bool IsJudgementThresholdReached(int i)
            => RuntimeData.HasJudgementTimerOfIndexExceeded(i);

        private void SetCarryDelayTimer(float delay) => carryDelayTimer = Time.time + delay;
        private bool CarryDelayTimerReached => Time.time > carryDelayTimer;


        #endregion
    }
}
