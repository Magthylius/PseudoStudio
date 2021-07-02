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
        private BTSelector aggRoot;

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

            // BTSequence aggD4 = null;
            // BTSequence aggD3 = null;
            // BTSequence aggD2 = null;
            // BTSequence aggD1 = null;
            BTSequence fallBack = Build_Sequence(new IsPlayersInCavernEqualToNode(Brain, 0), new ChangeStateNode(Brain, BrainState.Recovery)).WithDebugName(nameof(fallBack));

            //!Defensive Branch
            SetupDefensiveBranchBehaviourTree4(ref seqD4);
            SetupDefensiveBranchBehaviourTree3(ref seqD3);
            SetupDefensiveBranchBehaviourTree2(ref seqD2);
            SetupDefensiveBranchBehaviourTree1(ref seqD1);

            //!Offensive Branch
            // SetupOffensiveBranchBehaviourTree4(ref aggD4);
            // SetupOffensiveBranchBehaviourTree3(ref aggD3);
            // SetupOffensiveBranchBehaviourTree2(ref aggD2);
            // SetupOffensiveBranchBehaviourTree1(ref aggD1);

            defRoot = Build_Selector(
                seqD4,
                seqD3,
                seqD2,
                seqD1,
                fallBack
            ).WithDebugName(nameof(defRoot));

            // aggRoot = Build_Selector(
            //     aggD4,
            //     aggD3,
            //     aggD2,
            //     aggD1,
            //     fallBack
            // ).WithDebugName(nameof(aggRoot));
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
            BTSequence ThreshIfJT3HaventPass = Build_Sequence(hasJT3NotPass, moveToPlayer, carryPlayer, setRecoveryState).WithDebugName(nameof(ThreshIfJT3HaventPass));
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
            HasJudgementThresholdNotExceededNode hasJT2NotPassed = new HasJudgementThresholdNotExceededNode(Brain, 2);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode threePlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 3);
            
            BTSequence CarryAndEscape = Build_Sequence(hasJT2NotPassed, moveToPlayer, carryPlayer, setRecoveryState).WithDebugName(nameof(CarryAndEscape));
            BTSelector IsCarryingPlayerWithFallback = Build_Selector(isCarryingPlayer, CarryAndEscape, setRecoveryState).WithDebugName(nameof(IsCarryingPlayerWithFallback));
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

            BTSequence CarryAndEscape = Build_Sequence(moveToPlayer, carryPlayer, setRecoveryState).WithDebugName(nameof(CarryAndEscape));
            BTSelector RecoveryAfterJT1NotPassed = Build_Selector(hasJT1NotPassed, setRecoveryState).WithDebugName(nameof(RecoveryAfterJT1NotPassed));
            BTSequence ThreshPlayerIfJT1HasNotPassed = Build_Sequence(RecoveryAfterJT1NotPassed, CarryAndEscape).WithDebugName(nameof(ThreshPlayerIfJT1HasNotPassed));

            D4Sequence = Build_Sequence(fourPlayerInCavern, ThreshPlayerIfJT1HasNotPassed).WithDebugName(nameof(D4Sequence));

        }
        #endregion

        #region Offensive Branch
        private void SetupOffensiveBranchBehaviourTree1(ref BTSequence A1Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT4Passed = new HasJudgementThresholdExceededNode(Brain, 4);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode onePlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 1);
            ModifyConfidenceNode snowballAgg = new ModifyConfidenceNode(Brain, 10, true);
            ModifyConfidenceNode retreatAgg = new ModifyConfidenceNode(Brain, 10, false);

            BTSequence retreatAndRecovery = Build_Sequence(retreatAgg, setRecoveryState);
            BTSelector CheckIfJT4Passed = Build_Selector(hasJT4Passed, retreatAndRecovery);
            BTSequence ThreshPlayerIfJT4HasNotPassed = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, snowballAgg);

            A1Sequence = Build_Sequence(onePlayerInCavern, CheckIfJT4Passed, ThreshPlayerIfJT4HasNotPassed);
        }

        private void SetupOffensiveBranchBehaviourTree2(ref BTSequence A2Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT3Passed = new HasJudgementThresholdExceededNode(Brain, 3);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode twoPlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 2);
            ModifyConfidenceNode snowballAgg = new ModifyConfidenceNode(Brain, 10, true);
            ModifyConfidenceNode retreatAgg = new ModifyConfidenceNode(Brain, 10, false);

            BTSequence retreatAndRecovery = Build_Sequence(retreatAgg, setRecoveryState);
            BTSelector CheckIfJT3Passed = Build_Selector(hasJT3Passed, retreatAndRecovery);
            BTSequence ThreshPlayerIfJT3HasNotPassed = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, snowballAgg);

            A2Sequence = Build_Sequence(twoPlayerInCavern, CheckIfJT3Passed, ThreshPlayerIfJT3HasNotPassed);
        }

        private void SetupOffensiveBranchBehaviourTree3(ref BTSequence A3Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT2Passed = new HasJudgementThresholdExceededNode(Brain, 2);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode threePlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 3);
            ModifyConfidenceNode snowballAgg = new ModifyConfidenceNode(Brain, 10, true);
            ModifyConfidenceNode retreatAgg = new ModifyConfidenceNode(Brain, 10, false);

            BTSequence retreatAndRecovery = Build_Sequence(retreatAgg, setRecoveryState);
            BTSelector CheckIfJT2Passed = Build_Selector(hasJT2Passed, retreatAndRecovery);
            BTSequence ThreshPlayerIfJT2HasNotPassed = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, snowballAgg);

            A3Sequence = Build_Sequence(threePlayerInCavern, CheckIfJT2Passed, ThreshPlayerIfJT2HasNotPassed);

        }

        private void SetupOffensiveBranchBehaviourTree4(ref BTSequence A4Sequence)
        {
            ChangeStateNode setRecoveryState = new ChangeStateNode(Brain, BrainState.Recovery);
            ThreshCarriedPlayerNode threshCarriedPlayer = new ThreshCarriedPlayerNode(Brain, damageManager).WithDebugName(nameof(threshCarriedPlayer));
            IsCarryingAPlayerNode isCarryingPlayer = new IsCarryingAPlayerNode(Brain, false);
            HasJudgementThresholdExceededNode hasJT1Passed = new HasJudgementThresholdExceededNode(Brain, 1);
            CarryTargetNode carryPlayer = new CarryTargetNode(Brain, 10, 0.5f);
            MoveToPlayerNode moveToPlayer = new MoveToPlayerNode(Brain, Brain.RuntimeData.navPointPrefab, 10, 1000, false).WithDebugName(nameof(moveToPlayer));
            IsPlayersInCavernEqualToNode fourPlayerInCavern = new IsPlayersInCavernEqualToNode(Brain, 4);
            ModifyConfidenceNode snowballAgg = new ModifyConfidenceNode(Brain, 10, true);
            ModifyConfidenceNode retreatAgg = new ModifyConfidenceNode(Brain, 10, false);

            BTSequence retreatAndRecovery = Build_Sequence(retreatAgg, setRecoveryState);
            BTSelector CheckIfJT1Passed = Build_Selector(hasJT1Passed, retreatAndRecovery);
            BTSequence ThreshPlayerIfJT1HasNotPassed = Build_Sequence(moveToPlayer, carryPlayer, threshCarriedPlayer, snowballAgg);

            A4Sequence = Build_Sequence(fourPlayerInCavern, CheckIfJT1Passed, ThreshPlayerIfJT1HasNotPassed);
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
            
            Brain.TriggerJudgementStateEvent(true);
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
            Brain.TriggerJudgementStateEvent(false);
        }
        public override Func<bool> ShouldTerminate() => () => false;

        private void ResetUpdateTimer() => updateTimer = 0.0f;
        private float TickUpdateTimer(in float tick) => updateTimer += tick;

        private BTSelector Build_Selector(params BTNode[] nodes) => new BTSelector(new List<BTNode>(nodes));
        private BTSequence Build_Sequence(params BTNode[] nodes) => new BTSequence(new List<BTNode>(nodes));
        private BTSuccessor Build_Successor(params BTNode[] nodes) => new BTSuccessor(new List<BTNode>(nodes));

    }
}
