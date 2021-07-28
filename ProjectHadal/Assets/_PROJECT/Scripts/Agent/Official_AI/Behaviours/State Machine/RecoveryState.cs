using System.Collections;
using UnityEngine;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

//! C: Jon
namespace Hadal.AI.States
{
    public class RecoveryState : AIStateBase
    {
        RecoveryStateSettings settings;
        private int judgementLapseCount;
        private float currentEscapeTargetTime;
        private bool escapeNow = false;

        public RecoveryState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Recovery;
            judgementLapseCount = 0;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();

            RuntimeData.UpdateCumulativeDamageCountThreshold(settings.G_DisruptionDamageCount);
            RuntimeData.ResetCumulativeDamageCount();
            NavigationHandler.SetSpeedMultiplier(settings.EscapeSpeedMultiplier);
            AllowStateTick = true;
            escapeNow = false;

            //! Get randomised linger timer & make sure it is always lower than the max escape time
            currentEscapeTargetTime = settings.GetLingerInCurrentCavernTime();
            if (currentEscapeTargetTime >= settings.MaxEscapeTime)
                currentEscapeTargetTime = settings.MaxEscapeTime - 1f;
        }

        public override void StateTick()
        {
            if (!AllowStateTick) return;

            if (!Brain.IsStunned)
            {
                RuntimeData.TickRecoveryTicker(Brain.DeltaTime);
                if (Brain.CheckForAIAndPlayersInTunnel()) return;
            }

            if (CheckForEscapeLingerTime()) return;
            if (CheckForLastResortCase()) return;
        }

        public override void LateStateTick()
        {
        }
        public override void FixedStateTick()
        {
        }
        public override void OnStateEnd()
        {
            RuntimeData.ResetRecoveryTicker();
            NavigationHandler.ResetSpeedMultiplier();
            AllowStateTick = false;
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            if (cavern == Brain.TargetMoveCavern)
            {
                if (cavern.GetPlayerCount > 1) SetNewTargetCavern();
                else if (RuntimeData.GetRecoveryTicks >= settings.MinimumRecoveryTime && cavern.GetPlayerCount <= 0)
                {
                    RuntimeData.SetBrainState(BrainState.Cooldown);
                    RuntimeData.ResetRecoveryTicker();
                }
            }
            else
            {
                DetermineNextCavern();
            }

            NavigationHandler.CavernModeSteering();
        }

        public override void OnCavernLeave(CavernHandler cavern)
        {
            NavigationHandler.TunnelModeSteering();
        }

        public override void OnPlayerEnterAICavern(CavernPlayerData data)
        {
            SetNewTargetCavern();
        }

        #region State specific

        void SetNewTargetCavern()
        {
            CavernHandler targetCavern = Brain.CavernManager.GetLeastPopulatedCavern(Brain.CavernManager.GetHandlerListExcludingAI());
            Brain.UpdateTargetMoveCavern(targetCavern);

            CavernManager.SeedCavernHeuristics(targetCavern);
            DetermineNextCavern();
        }

        private Coroutine cavernRoutine = null;
        void DetermineNextCavern()
        {
            if (cavernRoutine != null)
                Brain.StopCoroutine(cavernRoutine);

            cavernRoutine = null;
            cavernRoutine = Brain.StartCoroutine(WaitForAICavern());

            IEnumerator WaitForAICavern()
            {
                while (AICavern == null)
                    yield return null;

                CavernHandler nextCavern = CavernManager.GetNextBestCavern(AICavern, true);
                NavigationHandler.SetImmediateDestinationToCavern(nextCavern);
                Brain.UpdateNextMoveCavern(nextCavern);
            }
        }

        private bool CheckForEscapeLingerTime()
        {
            if (escapeNow)
                return false;

            if (RuntimeData.GetRecoveryTicks >= currentEscapeTargetTime)
            {
                escapeNow = true;
                SetNewTargetCavern();
                return true;
            }
            return false;
        }

        private bool CheckForLastResortCase()
        {
            //! When hit too much or escape time is too long, do a last resort check
            if (RuntimeData.GetRecoveryTicks >= settings.MaxEscapeTime || RuntimeData.IsCumulativeDamageCountReached)
            {
                SenseDetection.RequestImmediateSensing();
                bool anyPlayersInAICavern = AICavern != null && AICavern.GetPlayerCount > 0;
                bool anyPlayersInDetection = SenseDetection.DetectedPlayersCount > 0;

                if (anyPlayersInAICavern || anyPlayersInDetection)
                {
                    RuntimeData.UpdateConfidenceValue(-settings.ConfidenceDecrementValue);
                    RuntimeData.ResetRecoveryTicker();
                    AllowStateTick = false;

                    if (Brain.CheckForJudgementStateCondition(out var OnSuccess))
                    {
                        if (RuntimeData.IsPreviousBrainStateEqualTo(BrainState.Judgement))
                            judgementLapseCount++;
                        else
                            judgementLapseCount = 0;

                        if (judgementLapseCount < settings.G_JudgementLapseCountLimit)
                        {
                            OnSuccess?.Invoke();
                            return true;
                        }
                    }

                    //! if did not detect any target players or judgement lapse limit reached: go to cooldown state
                    RuntimeData.SetBrainState(BrainState.Cooldown);
                    judgementLapseCount = 0;
                    return true;
                }
            }
            return false;
        }

        #endregion

        public override Func<bool> ShouldTerminate() => () => false;
    }
}