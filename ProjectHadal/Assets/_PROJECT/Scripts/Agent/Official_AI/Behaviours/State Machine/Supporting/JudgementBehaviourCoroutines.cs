using System.Collections;
using Castle.Components.DictionaryAdapter.Xml;
using Hadal.AI.Caverns;
using Hadal.AI.States;
using Hadal.Player;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;
using Action = System.Action;

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
        }

        private float carryDelayTimer;
        private bool canCarry;
        private bool isAttacking;
        private bool isDamaging;
        private CoroutineData threshRoutineData;

        #region Coroutines

        public IEnumerator DoThreshAttack()
        {
            isDamaging = true;
            void StopAttack() => isDamaging = false;

            DamageManager.ApplyDoT(Brain.CarriedPlayer,
                Settings.G_TotalThreshTimeInSeconds,
                Settings.G_ThreshDamagePerSecond,
                StopAttack);

            while (isDamaging && DamageManager != null)
                yield return null;

            TryDebug("Thresh damage in inner routine is finished!");
        }

        public IEnumerator DefensiveStance(int stanceIndex)
        {
            TryDebug($"Starting Defensive Behaviour for player count: {stanceIndex}.");
            JState.IsBehaviourRunning = true;
            bool waitForJtimer = false;
            int jTimerIndex = 5 - stanceIndex;

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
                    if (!success)
                    {
                        waitForJtimer = true;
                        break;
                    }
                }

                //! Thresh if is carrying a player & is not yet threshing
                if (Brain.IsCarryingAPlayer() && !isAttacking)
                {
                    isAttacking = true;
                    WaitForSeconds waitTime = new WaitForSeconds(0.5f);

                    TryDebug("Starting threshing routine.");
                    threshRoutineData = new CoroutineData(Brain, DoThreshAttack());
                    yield return threshRoutineData.Coroutine; //this will wait for the DoThreshAttack() coroutine to finish

                    TryDebug("Thresh damage in outer routine is finished!");

                    // DamageManager.ApplyDoT(Brain.CarriedPlayer,
                    //     Settings.G_TotalThreshTimeInSeconds,
                    //     Settings.G_ThreshDamagePerSecond,
                    //     StopAttack);

                    // while (isDamaging)
                    //     yield return waitDoTTime;

                    bool success = Brain.TryDropCarriedPlayer();
                    TryDebug("Attacking is done, dropping carried player. Stopping behaviour.");
                    break;

                    


                }

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
                    SetCarryDelayTimer();
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

                yield return null;
            }

            //! If the wait boolean is true, will wait until the judgement timer is exceeded before handling behaviour end
            while (!IsJudgementThresholdReached(jTimerIndex) && waitForJtimer)
                yield return null;

            //! Handle Behaviour ending
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            yield return null;
        }

        public IEnumerator AggressiveStance(int stanceIndex)
        {
            TryDebug($"Starting Aggressive Behaviour for player count: {stanceIndex}.");
            JState.IsBehaviourRunning = true;
            bool waitForJtimer = false;
            int jTimerIndex = 5 - stanceIndex;

            while (JState.IsBehaviourRunning)
            {

                yield return null;
            }

            //! If the wait boolean is true, will wait until the judgement timer is exceeded before handling behaviour end
            while (!IsJudgementThresholdReached(jTimerIndex) && waitForJtimer)
                yield return null;

            //! Handle Behaviour ending
            ResetStateValues();
            RuntimeData.SetBrainState(BrainState.Recovery);
            yield return null;
        }

        #endregion

        #region Utility & Verbose Shorthands

        private bool TrySetCustomNavPoint(PlayerController player)
        {
            NavPoint point = player.GetComponentInChildren<NavPoint>();
            if (point == null)
            {
                point = Object.Instantiate(RuntimeData.navPointPrefab, player.GetTarget.position, Quaternion.identity);
                point.tag = "NavigationPoint";
                point.AttachTo(player.transform);
                point.SetCavernTag(CavernTag.Custom_Point);
                NavigationHandler.SetCustomPath(point, true);
                return true;
            }
            return false;
        }

        private void ResetStateValues()
        {
            carryDelayTimer = 0f;
            canCarry = false;
            isAttacking = false;
            isDamaging = false;
            threshRoutineData = null;
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
            => CavernManager.GetHandlerOfAILocation.GetPlayerCount == 0;

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

        private void SetCarryDelayTimer() => carryDelayTimer = Time.time + Settings.G_CarryDelayTimer;
        private bool CarryDelayTimerReached => Time.time > carryDelayTimer;


        #endregion
    }
}
