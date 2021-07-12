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
        private AISenseDetection SenseDetection;
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
            SenseDetection = Brain.SenseDetection;
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
        private Coroutine damageManagerRoutine;
        private CoroutineData threshRoutineData;
        private CoroutineData approachRoutineData;

        #region Coroutines

        /// <summary>
        /// Will attempt to place a custom nav point onto the target player and will detect whether it fulfills the close and far
        /// distance to proceed into threshing or stopping behaviour respectively.
        /// </summary>
        /// <param name="carryDelayTime">A custom time used to make the AI wait for a while before actually carrying the target. It
        /// is usually made less than 1 second. </param>
        private IEnumerator MoveToCurrentTarget(float carryDelayTime)
        {
            bool HasTargetIsolatedPlayer() => JState.IsolatedPlayer != null;
            bool HasCurrentTargetPlayer() => Brain.CurrentTarget != null;
            bool targetMarked = false;

            while (JState.IsBehaviourRunning)
            {
                //! Set custom nav point to destination: current target/isolated player if not already moving towards it.
                if ((HasCurrentTargetPlayer() || HasTargetIsolatedPlayer()) && !targetMarked)
                {
                    bool success;
                    if (JState.IsolatedPlayer != null)
                        success = TrySetCustomNavPoint(JState.IsolatedPlayer);
                    else
                        success = TrySetCustomNavPoint(Brain.CurrentTarget);

                    Brain.AudioBank.Play3D(soundType: AISound.Thresh, Brain.transform);
                    if (success)
                    {
                        targetMarked = true;
                        TryDebug("Set custom nav point onto target. Moving to chase target.");
                    }
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
                        JState.IsBehaviourRunning = false;
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

        /// <summary>
        /// Threshing attack that will apply damage every second to the player as long as the coroutine is running. Will succeed if
        /// it threshing all the way to the end; will fail if there is any interuption (e.g. cumulated damage threshold reached / stunned).
        /// It will call the respective success or failure callbacks when the coroutine exits.
        /// </summary>
        /// <param name="dps">The amount of damage to do to the carried player PER SECOND. </param>
        /// <param name="successCallback">A callback that will call if the threshing succeeds all the way through. Will not call on a failure case. </param>
        /// <param name="failureCallback">A callback that will call if the threshing is interupted by the cumulated damage threshold. It will NOT 
        /// call if the AI is stunned. </param>
        private IEnumerator DoThreshAttack(int dps, System.Action successCallback, System.Action failureCallback)
        {
            bool success = false;
            int totalDamageSeconds = Settings.G_TotalThreshTimeInSeconds;
            isDamaging = true;
            NavigationHandler.DisableWithLerp(2f);

            void StopAttack()
            {
                isDamaging = false;
                success = true;
            }

            damageManagerRoutine = DamageManager.ApplyDoT(Brain.CarriedPlayer,
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

        /// <summary>
        /// Defensive Judgement behaviour that will manage the <see cref="MoveToCurrentTarget"/> and <see cref="DoThreshAttack"/> coroutines
        /// in the execution. It will personally handle the transition between both of these coroutines up until the end of the behaviour.
        /// </summary>
        /// <param name="stanceIndex">Index passed in should be the number of players detected in the same cavern OR in close vicinity. </param>
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
                    TryDebug("No more players detected in cavern or nearby, stopping defensive behaviour.");
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
                            null,
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
            approachRoutineData.Stop();
            approachRoutineData = null;
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            
            yield return null;
        }

        /// <summary>
        /// Aggressive Judgement behaviour that will manage the <see cref="MoveToCurrentTarget"/> and <see cref="DoThreshAttack"/> coroutines
        /// in the execution. It will personally handle the transition between both of these coroutines up until the end of the behaviour.
        /// </summary>
        /// <param name="stanceIndex">Index passed in should be the number of players detected in the same cavern OR in close vicinity. </param>
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
                    TryDebug("No more players detected in cavern or nearby, stopping aggressive behaviour.");
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
            approachRoutineData.Stop();
            approachRoutineData = null;
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);

            yield return null;
        }

        /// <summary>
        /// Ambushing behaviour facilitated through Judgement that will manage the <see cref="MoveToCurrentTarget"/> and <see cref="DoThreshAttack"/>
        /// coroutines in the execution. It will personally handle the transition between both of these coroutines up until the end of the behaviour.
        /// </summary>
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

        /// <summary>
        /// Tries to tag a custom nav point onto the passed in player in which the AI should chase. It will return True if the tagging is done
        /// (spawned a custom nav point and informed the navigation handler to chase); it will return False if the player has already been tagged.
        /// </summary>
        /// <param name="player">Player to tag.</param>
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

        /// <summary>
        /// Handles the immediate case in which the AI is stunned and stops its current behaviour. It will also decide whether it should
        /// remain in Judgement state or proceed to Recovery state, after the stun ends, with a random chance.
        /// </summary>
        private void HandleStunEvent(bool isStunned)
        {
            if (!isStunned)
                return;

            if (damageManagerRoutine != null) Brain.StopCoroutine(damageManagerRoutine);
            damageManagerRoutine = null;
            threshRoutineData?.Stop();      threshRoutineData = null;
            approachRoutineData?.Stop();    approachRoutineData = null;
            JState.IsBehaviourRunning = false;
            JState.StopAnyRunningCoroutines();
            NavigationHandler.Enable();
            NavigationHandler.ResetSpeedMultiplier();
            ResetStateValues();
            var brainState = GetRandomBrainStateAfterStun();
            RuntimeData.SetBrainState(brainState);
            
            string debugMsg = "The Leviathan has been stunned. Stopping behaviour ";
            debugMsg += brainState == BrainState.Judgement ? "but has chosen to remain in Judgement state." : "and has chosen to go to Recovery state.";
            TryDebug(debugMsg);
        }

        /// <summary> Resets values used in the behaviour coroutines so it can be reused another time. </summary>
        public void ResetStateValues()
        {
            carryDelayTimer = 0f;
            canCarry = false;
            isAttacking = false;
            isDamaging = false;
            JState.ResetStateValues();
        }

        /// <summary>
        /// Facilitates the random chance event to choose between Judgement state or Recovery state. Returns the result of this random
        /// chance.
        /// </summary>
        private BrainState GetRandomBrainStateAfterStun()
        {
            bool shouldStayJudgement = Settings.PostStunRemainJudgementChance.HasHitPercentChance();
            return shouldStayJudgement ? BrainState.Judgement : BrainState.Recovery;
        }

        private void TryDebug(object msg)
        {
            if (Brain.DebugEnabled)
            {
                msg = "Judgement: " + msg;
                msg.Msg();
            }
        }

        private bool NoMorePlayersInCavern => JState.AICavern != null && JState.AICavern.GetPlayerCount == 0;
        private bool NoMorePlayersNearby => SenseDetection.DetectedPlayersCount == 0;

        private bool PlayerCountDroppedTo0 => NoMorePlayersInCavern && NoMorePlayersNearby;

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
