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
        AIBrain b;
        AIDamageManager damageManager;
        BTSequence root;
        float updateTimer;
        float updateDelay;

        public JudgementSubState()
        {
            ResetUpdateTimer();
        }
        public void Initialise(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
            updateDelay = 1f / b.MachineData.Engagement.JudgementTickRate;

            BTNode.EnableDebug = b.DebugEnabled;
            BTNode.ExecutionOrder = 0;
            Test_SetupDefensiveBranchBehaviourTree();
            Test_SetupOffensiveBranchBehaviourTree1();
            root.SetDebugName("Root");
        }

        //! FILO
        private void Test_SetupDefensiveBranchBehaviourTree()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence tailWhip = new BTSequence(new List<BTNode>() { new TailWhipNode(b) });
            tailWhip.SetDebugName("tail whip");

            BTSequence escapeTailWhip = new BTSequence(new List<BTNode>() { tailWhip, setRecoveryState });
            escapeTailWhip.SetDebugName("escape tail whip");

            BTSelector hasJT4Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 4) });
            hasJT4Passed.SetDebugName("has jt4 passed?");

            BTSequence recoveryAfterJT4Passed = new BTSequence(new List<BTNode>() { hasJT4Passed, setRecoveryState });
            recoveryAfterJT4Passed.SetDebugName("recovery after jt4 passed");

            BTSequence threshCarriedPlayer = new BTSequence(new List<BTNode>() { new ThreshCarriedPlayerNode(b, damageManager) });
            threshCarriedPlayer.SetDebugName("thresh carried player");

            BTSelector tryToThreshCarriedPlayer = new BTSelector(new List<BTNode>() { threshCarriedPlayer, recoveryAfterJT4Passed });
            tryToThreshCarriedPlayer.SetDebugName("try to thresh carried player?");

            BTSelector threshAndRecoveryIfSuccessful = new BTSelector(new List<BTNode>() { tryToThreshCarriedPlayer, setRecoveryState });
            threshAndRecoveryIfSuccessful.SetDebugName("Thresh & Recovery?");

            BTSelector onePlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 1) });
            onePlayerInCavern.SetDebugName("One player in cavern?");

            BTSequence isCarryingAnyPlayer = new BTSequence(new List<BTNode>() { new IsCarryingAPlayerNode(b, false) });
            isCarryingAnyPlayer.SetDebugName("Is carrying any player");

            BTSelector carryOrThresh = new BTSelector(new List<BTNode>() { isCarryingAnyPlayer, threshAndRecoveryIfSuccessful });
            carryOrThresh.SetDebugName("Carry Or Thresh?");

            BTSequence sequenceD1 = new BTSequence(new List<BTNode>()
            {
                onePlayerInCavern,
                carryOrThresh,
                hasJT4Passed,
                escapeTailWhip
            });
            sequenceD1.SetDebugName("sequence D1");

            root = new BTSequence(new List<BTNode>() { sequenceD1 });
        }

        private void Test_SetupOffensiveBranchBehaviourTree1()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence increaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, true) });
            increaseConfidence.SetDebugName("increase confidence");

            //Fallback if threshNearestTarget fails
            BTSelector hasJT4Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 4) });
            hasJT4Passed.SetDebugName("has jt4 passed?");

            BTSequence decreaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, false) });
            decreaseConfidence.SetDebugName("decrease confidence");

            BTSequence resetCumulativeDamageThreshold = new BTSequence(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b) });
            resetCumulativeDamageThreshold.SetDebugName("reset cumulative damage threshold");

            BTSequence threshFallbackA1 = new BTSequence(new List<BTNode>()
            {
                hasJT4Passed,
                decreaseConfidence,
                setRecoveryState
            });
            threshFallbackA1.SetDebugName("thresh fallback A1");

            BTSequence getPlayerToCarry = new BTSequence(new List<BTNode>() { new MoveToPlayerNode(b, b.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(b, 1.5f, 0.5f) });
            getPlayerToCarry.SetDebugName("get player to carry");

            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false), getPlayerToCarry });
            isCarryingAnyPlayer.SetDebugName("is carrying any player?");

            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b, damageManager), threshFallbackA1 });
            threshCarriedPlayer.SetDebugName("thresh carried player?");

            BTSequence threshNearestTarget = new BTSequence(new List<BTNode>() { isCarryingAnyPlayer, threshCarriedPlayer });
            threshNearestTarget.SetDebugName("thresh nearest target");

            BTSelector onePlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 1) });
            onePlayerInCavern.SetDebugName("one player in cavern?");

            BTSequence postSequenceA1 = new BTSequence(new List<BTNode>() { increaseConfidence, resetCumulativeDamageThreshold });
            postSequenceA1.SetDebugName("post sequence A1");

            BTSequence sequenceA1 = new BTSequence(new List<BTNode>()
            {
                onePlayerInCavern,
                threshNearestTarget,
                postSequenceA1
            });
            sequenceA1.SetDebugName("sequence A1");

            root.AddNode(sequenceA1);
        }

        public void OnStateStart()
        {
            if (b.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
            b.RuntimeData.ResetJudgementTimer();
        }
        public void StateTick()
        {
            float deltaTime = b.DeltaTime;
            b.RuntimeData.TickJudgementTimer(deltaTime);
            if (TickUpdateTimer(deltaTime) > updateDelay)
            {
                ResetUpdateTimer();
                var result = root.Evaluate();
                if (b.DebugEnabled)
                {
                    if (result == NodeState.RUNNING) "Tree: Running".Msg();
                    else if (result == NodeState.SUCCESS) "Tree: Success".Msg();
                    else if (result == NodeState.FAILURE) "Tree: Fail".Msg();
                }
            }

            // Behaviour tree links
            // Logic Diagram https://app.diagrams.net/#G1uh0jwavfwoBIC7Pb8agoDO-CJPGdlkac
            // Node Documentation https://app.diagrams.net/#G1S3qrdiuVc7uVjAx3LYDiG1rLkVtIGEKE
        }
        public void LateStateTick() { }
        public void FixedStateTick() { }
        public void OnStateEnd() { }
        public Func<bool> ShouldTerminate() => () => false;

        private void ResetUpdateTimer() => updateTimer = 0.0f;
        private float TickUpdateTimer(in float tick) => updateTimer += tick;
    }
}
