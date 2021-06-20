using Tenshi.AIDolls;
using System;
using Hadal.AI.TreeNodes;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using Tenshi;
using UnityEngine;

namespace Hadal.AI.States
{
    public class JudgementSubState : AIStateBase
    {
        EngagementState parent;
        AIDamageManager damageManager;
        EngagementStateSettings engagementStateSettings;

        private BTSelector defRoot;

        float updateTimer;
        float updateDelay;

        public JudgementSubState()
        {
            ResetUpdateTimer();
        }
        public void Initialise(EngagementState parent)
        {
            this.parent = parent;
            Initialize(parent.Brain);
            Brain = parent.Brain;
            damageManager = Brain.DamageManager;
            updateDelay = 1f / Brain.MachineData.Engagement.JudgementTickRate;

            BTNode.EnableDebug = Brain.DebugEnabled;

            BTSequence seqD1 = null;
            BTSequence seqD2 = null;
            BTSequence seqD3 = null;
            BTSequence seqD4 = null;
            BTSequence fallBack = Build_Sequence(new IsPlayersInCavernEqualToNode(Brain, 0), new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(fallBack));

            //!Defensive
            SetupDefensiveBranchBehaviourTree4(ref seqD4);
            SetupDefensiveBranchBehaviourTree3(ref seqD3);
            SetupDefensiveBranchBehaviourTree2(ref seqD2);
            SetupDefensiveBranchBehaviourTree1(ref seqD1);

            defRoot = Build_Selector(
                seqD4,
                seqD3,
                seqD2,
                seqD1,
                fallBack
            ).WithDebugName(nameof(defRoot));

            //SetupDefensiveBranchBehaviourTree3();
            //SetupDefensiveBranchBehaviourTree4();


            // //!Offensive
            // SetupOffensiveBranchBehaviourTree1();
            // SetupOffensiveBranchBehaviourTree2();
            // SetupOffensiveBranchBehaviourTree3();
            // SetupOffensiveBranchBehaviourTree4();
            // rootAgg.WithDebugName("RootAgg");
        }


        //! FILO
        //! Setup Def/Off Branch BT X (X based on number of players to check)
        #region Defensive Branch
        private void SetupDefensiveBranchBehaviourTree1(ref BTSequence D1Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            TailWhipNode tailWhip = new TailWhipNode(Brain, 5f);
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsPlayersInCavernEqualToNode onePlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 1);
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT4Passed = new HasJudgementThresholdExceededNode(Brain, 4);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);

            BTSequence RecoveryAfterJT4Passed = Build_Sequence(hasJT4Passed, setRecoveryState).WithDebugName(nameof(RecoveryAfterJT4Passed));
            BTSequence ThreshIfPlayerCarryFails = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(ThreshIfPlayerCarryFails));
            BTSelector isCarryingPlayerWithFallback = Build_Selector(isCarryingPlayer, ThreshIfPlayerCarryFails, RecoveryAfterJT4Passed).WithDebugName(nameof(isCarryingPlayerWithFallback));

            D1Sequence = Build_Sequence(onePlayerInCavern, isCarryingPlayerWithFallback, threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(D1Sequence));
        }

        //! Notes: Detect two player, IsCarrying?, Has jt3 passed?, escape whip
        //! If IsCarrying fails, Grab nearest player and set to recovery, if grab fails, escape
        //! If jt3 fails, thresh nearest player and set to recovery, if thresh fails, wait for jt1+jt3 pass and set to recovery
        private void SetupDefensiveBranchBehaviourTree2(ref BTSequence D2Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            TailWhipNode tailWhip = new TailWhipNode(Brain, 5f).WithDebugName(nameof(TailWhipNode));
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdNotExceededNode hasJT3NotPass = new HasJudgementThresholdNotExceededNode(Brain, 3);
            HasJudgementThresholdExceededNode hasJT3Pass = new HasJudgementThresholdExceededNode(Brain, 3);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode twoPlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 2);

            //BTSequence TailWhipAndRecover = Build_Sequence(tailWhip, setRecoveryState).WithDebugName(nameof(TailWhipAndRecover));
            BTSequence ThreshIfJT3HaventPass = Build_Sequence(hasJT3NotPass, moveToPlayer, carryPlayer, threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(ThreshIfJT3HaventPass));
            BTSelector IsCarryingPlayerWithFallback = Build_Selector(isCarryingPlayer, ThreshIfJT3HaventPass, setRecoveryState).WithDebugName(nameof(IsCarryingPlayerWithFallback));
            BTSequence ThreshCarriedPlayerAndEscape = Build_Sequence(threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(ThreshCarriedPlayerAndEscape));

            D2Sequence = Build_Sequence(twoPlayerInCavern, IsCarryingPlayerWithFallback, ThreshCarriedPlayerAndEscape).WithDebugName(nameof(D2Sequence));

        }

        private void SetupDefensiveBranchBehaviourTree3(ref BTSequence D3Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            TailWhipNode tailWhip = new TailWhipNode(Brain, 5f).WithDebugName(nameof(TailWhipNode));
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT2Passed = new HasJudgementThresholdExceededNode(Brain, 2);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode threePlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 3);

            BTSequence ThreshPlayer = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(ThreshPlayer));
            BTSequence RecoverIfJT2HasNotPassed = Build_Sequence(hasJT2Passed, setRecoveryState).WithDebugName(nameof(RecoverIfJT2HasNotPassed));
            BTSelector IsCarryingPlayerWithFallback = Build_Selector(isCarryingPlayer, RecoverIfJT2HasNotPassed, ThreshPlayer).WithDebugName(nameof(IsCarryingPlayerWithFallback));
            BTSequence ThreshCarriedPlayerAndEscape = Build_Sequence(threshCarriedPlayer, setRecoveryState).WithDebugName(nameof(ThreshCarriedPlayerAndEscape));

            D3Sequence = Build_Sequence(threePlayerInCavern, IsCarryingPlayerWithFallback, ThreshCarriedPlayerAndEscape).WithDebugName(nameof(D3Sequence));
        }

        private void SetupDefensiveBranchBehaviourTree4(ref BTSequence D4Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            TailWhipNode tailWhip = new TailWhipNode(Brain, 5f).WithDebugName(nameof(TailWhipNode));
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdNotExceededNode hasJT1NotPassed = new HasJudgementThresholdNotExceededNode(Brain, 1);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode fourPlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 4);

            BTSequence ThreshPlayer = Build_Sequence(moveToPlayer, carryPlayer, setRecoveryState).WithDebugName(nameof(ThreshPlayer));
            BTSelector RecoveryAfterJT1NotPassed = Build_Selector(hasJT1NotPassed, setRecoveryState).WithDebugName(nameof(RecoveryAfterJT1NotPassed));
            BTSequence ThreshPlayerIfJT1HasNotPassed = Build_Sequence(RecoveryAfterJT1NotPassed, ThreshPlayer).WithDebugName(nameof(ThreshPlayerIfJT1HasNotPassed));

            D4Sequence = Build_Sequence(fourPlayerInCavern, ThreshPlayerIfJT1HasNotPassed).WithDebugName(nameof(D4Sequence));

        }
        #endregion

        #region Offensive Branch
        private void SetupOffensiveBranchBehaviourTree1()
        {
            BTSequence setRecoveryState = Build_Sequence(new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(setRecoveryState));
            BTSelector hasJt4Passed = Build_Selector(new HasJudgementThresholdExceededNode(Brain, 4)).WithDebugName(nameof(hasJt4Passed));
            BTSequence increaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, true)).WithDebugName(nameof(increaseConfidence));
            BTSequence decreaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, false)).WithDebugName(nameof(decreaseConfidence));
            BTSequence resetCumulativeDamageThreshold = Build_Sequence(new ResetCumulatedDamageThresholdNode(Brain, engagementStateSettings))
                .WithDebugName(nameof(resetCumulativeDamageThreshold));

            BTSequence threshFallback1 = Build_Sequence(hasJt4Passed, decreaseConfidence, setRecoveryState).WithDebugName(nameof(threshFallback1));
            BTSelector threshCarriedPlayer = Build_Selector(new ThreshCarriedPlayerNode(Brain, damageManager), threshFallback1).WithDebugName(nameof(threshCarriedPlayer));

            BTSequence getPlayerToCarry = Build_Sequence(new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(Brain, 1.5f, 0.5f))
                .WithDebugName(nameof(getPlayerToCarry));
            BTSelector isCarryingAnyPlayer = Build_Selector(new IsCarryingAPlayerNode(Brain, false), getPlayerToCarry).WithDebugName(nameof(isCarryingAnyPlayer));
            BTSequence threshNearestTarget = Build_Sequence(isCarryingAnyPlayer, threshCarriedPlayer).WithDebugName(nameof(threshNearestTarget));
            BTSelector tryThreshNearestTarget = Build_Selector(threshNearestTarget, hasJt4Passed).WithDebugName(nameof(tryThreshNearestTarget));

            BTSelector onePlayerInCavern = Build_Selector(new IsPlayersInCavernEqualToNode(Brain, 1)).WithDebugName(nameof(onePlayerInCavern));
            BTSequence postSequenceA1 = Build_Sequence(increaseConfidence, resetCumulativeDamageThreshold).WithDebugName(nameof(postSequenceA1));

            BTSequence sequenceA1 = Build_Sequence(
                onePlayerInCavern,
                tryThreshNearestTarget,
                postSequenceA1
            ).WithDebugName(nameof(sequenceA1));

        }

        private void SetupOffensiveBranchBehaviourTree2()
        {
            BTSequence setRecoveryState = Build_Sequence(new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(setRecoveryState));
            BTSelector hasJt3Passed = Build_Selector(new HasJudgementThresholdExceededNode(Brain, 3)).WithDebugName(nameof(hasJt3Passed));
            BTSequence increaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, true)).WithDebugName(nameof(increaseConfidence));
            BTSequence decreaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, false)).WithDebugName(nameof(decreaseConfidence));
            BTSequence resetCumulativeDamageThreshold = Build_Sequence(new ResetCumulatedDamageThresholdNode(Brain, engagementStateSettings))
                .WithDebugName(nameof(resetCumulativeDamageThreshold));

            BTSequence threshFallback1 = Build_Sequence(hasJt3Passed, decreaseConfidence, setRecoveryState).WithDebugName(nameof(threshFallback1));
            BTSelector threshCarriedPlayer = Build_Selector(new ThreshCarriedPlayerNode(Brain, damageManager), threshFallback1).WithDebugName(nameof(threshCarriedPlayer));

            BTSequence getPlayerToCarry = Build_Sequence(new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(Brain, 1.5f, 0.5f))
                .WithDebugName(nameof(getPlayerToCarry));
            BTSelector isCarryingAnyPlayer = Build_Selector(new IsCarryingAPlayerNode(Brain, false), getPlayerToCarry).WithDebugName(nameof(isCarryingAnyPlayer));
            BTSequence threshNearestTarget = Build_Sequence(isCarryingAnyPlayer, threshCarriedPlayer).WithDebugName(nameof(threshNearestTarget));
            BTSelector tryThreshNearestTarget = Build_Selector(threshNearestTarget, hasJt3Passed).WithDebugName(nameof(tryThreshNearestTarget));

            BTSelector twoPlayerInCavern = Build_Selector(new IsPlayersInCavernEqualToNode(Brain, 2)).WithDebugName(nameof(twoPlayerInCavern));
            BTSequence postSequenceA2 = Build_Sequence(increaseConfidence, resetCumulativeDamageThreshold).WithDebugName(nameof(postSequenceA2));

            BTSequence sequenceA2 = Build_Sequence(
                twoPlayerInCavern,
                tryThreshNearestTarget,
                postSequenceA2
            ).WithDebugName(nameof(sequenceA2));
        }

        private void SetupOffensiveBranchBehaviourTree3()
        {
            BTSequence setRecoveryState = Build_Sequence(new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(setRecoveryState));
            BTSelector hasJt2Passed = Build_Selector(new HasJudgementThresholdExceededNode(Brain, 2)).WithDebugName(nameof(hasJt2Passed));
            BTSequence increaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, true)).WithDebugName(nameof(increaseConfidence));
            BTSequence decreaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, false)).WithDebugName(nameof(decreaseConfidence));
            BTSequence resetCumulativeDamageThreshold = Build_Sequence(new ResetCumulatedDamageThresholdNode(Brain, engagementStateSettings))
                .WithDebugName(nameof(resetCumulativeDamageThreshold));

            BTSequence threshFallback1 = Build_Sequence(hasJt2Passed, decreaseConfidence, setRecoveryState).WithDebugName(nameof(threshFallback1));
            BTSelector threshCarriedPlayer = Build_Selector(new ThreshCarriedPlayerNode(Brain, damageManager), threshFallback1).WithDebugName(nameof(threshCarriedPlayer));

            BTSequence getPlayerToCarry = Build_Sequence(new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(Brain, 1.5f, 0.5f))
                .WithDebugName(nameof(getPlayerToCarry));
            BTSelector isCarryingAnyPlayer = Build_Selector(new IsCarryingAPlayerNode(Brain, false), getPlayerToCarry).WithDebugName(nameof(isCarryingAnyPlayer));
            BTSequence threshNearestTarget = Build_Sequence(isCarryingAnyPlayer, threshCarriedPlayer).WithDebugName(nameof(threshNearestTarget));
            BTSelector tryThreshNearestTarget = Build_Selector(threshNearestTarget, hasJt2Passed).WithDebugName(nameof(tryThreshNearestTarget));

            BTSelector threePlayerInCavern = Build_Selector(new IsPlayersInCavernEqualToNode(Brain, 3)).WithDebugName(nameof(threePlayerInCavern));
            BTSequence postSequenceA3 = Build_Sequence(increaseConfidence, resetCumulativeDamageThreshold).WithDebugName(nameof(postSequenceA3));

            BTSequence sequenceA3 = Build_Sequence(
                threePlayerInCavern,
                tryThreshNearestTarget,
                postSequenceA3
            ).WithDebugName(nameof(sequenceA3));

        }

        private void SetupOffensiveBranchBehaviourTree4()
        {
            BTSequence setRecoveryState = Build_Sequence(new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(setRecoveryState));
            BTSelector hasJt1Passed = Build_Selector(new HasJudgementThresholdExceededNode(Brain, 2)).WithDebugName(nameof(hasJt1Passed));
            BTSequence increaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, true)).WithDebugName(nameof(increaseConfidence));
            BTSequence decreaseConfidence = Build_Sequence(new ModifyConfidenceNode(Brain, 1, false)).WithDebugName(nameof(decreaseConfidence));
            BTSequence resetCumulativeDamageThreshold = Build_Sequence(new ResetCumulatedDamageThresholdNode(Brain, engagementStateSettings))
                .WithDebugName(nameof(resetCumulativeDamageThreshold));

            BTSequence threshFallback1 = Build_Sequence(hasJt1Passed, decreaseConfidence, setRecoveryState).WithDebugName(nameof(threshFallback1));
            BTSelector threshCarriedPlayer = Build_Selector(new ThreshCarriedPlayerNode(Brain, damageManager), threshFallback1).WithDebugName(nameof(threshCarriedPlayer));

            BTSequence getPlayerToCarry = Build_Sequence(new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(Brain, 1.5f, 0.5f))
                .WithDebugName(nameof(getPlayerToCarry));
            BTSelector isCarryingAnyPlayer = Build_Selector(new IsCarryingAPlayerNode(Brain, false), getPlayerToCarry).WithDebugName(nameof(isCarryingAnyPlayer));
            BTSequence threshNearestTarget = Build_Sequence(isCarryingAnyPlayer, threshCarriedPlayer).WithDebugName(nameof(threshNearestTarget));
            BTSelector tryThreshNearestTarget = Build_Selector(threshNearestTarget, hasJt1Passed).WithDebugName(nameof(tryThreshNearestTarget));

            BTSelector fourPlayerInCavern = Build_Selector(new IsPlayersInCavernEqualToNode(Brain, 4)).WithDebugName(nameof(fourPlayerInCavern));
            BTSequence postSequenceA4 = Build_Sequence(increaseConfidence, resetCumulativeDamageThreshold).WithDebugName(nameof(postSequenceA4));

            BTSequence sequenceA4 = Build_Sequence(
                fourPlayerInCavern,
                tryThreshNearestTarget,
                postSequenceA4
            ).WithDebugName(nameof(sequenceA4));
        }
        #endregion

        //TODO: Set a way to determine which root to go or probablity?
        int randomNumber;
        NodeState result;

        int DelayInterval;
        float tickTimer;
        void RandomizeAggOrDefRoot()
        {
            randomNumber = UnityEngine.Random.Range(0, 1);
        }
        public override void OnStateStart()
        {
            Initialise(parent);
            if (Brain.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
            Brain.RuntimeData.ResetEngagementTicker();
            //RandomizeAggOrDefRoot();

            //nextTick = Time.time + DelayInterval;
            tickTimer = 10;
            DelayInterval = 30;
        }

        public override void StateTick()
        {
            float deltaTime = Brain.DeltaTime;
            Brain.RuntimeData.TickEngagementTicker(deltaTime);

            //Debug.LogWarning("Judgement Tick!");

            if (Time.frameCount % DelayInterval == 0)
            {
                defRoot.Evaluate(deltaTime);
            }

            if (Brain.DebugEnabled)
            {
                //if (result == NodeState.RUNNING) "Tree: Running".Msg();
                //else if (result == NodeState.SUCCESS) "Tree: Success".Msg();
                //else if (result == NodeState.FAILURE) "Tree: Fail".Msg();
            }

            // Behaviour tree links
            // Logic Diagram https://app.diagrams.net/#G1uh0jwavfwoBIC7Pb8agoDO-CJPGdlkac
            // Node Documentation https://app.diagrams.net/#G1S3qrdiuVc7uVjAx3LYDiG1rLkVtIGEKE
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
            Brain.NavigationHandler.StopCustomPath(false);
        }
        public override Func<bool> ShouldTerminate() => () => false;

        private void ResetUpdateTimer() => updateTimer = 0.0f;
        private float TickUpdateTimer(in float tick) => updateTimer += tick;

        private BTSelector Build_Selector(params BTNode[] nodes) => new BTSelector(new List<BTNode>(nodes));
        private BTSequence Build_Sequence(params BTNode[] nodes) => new BTSequence(new List<BTNode>(nodes));
        private BTSuccessor Build_Successor(params BTNode[] nodes) => new BTSuccessor(new List<BTNode>(nodes));

    }
}
