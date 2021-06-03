using Tenshi.AIDolls;
using System;
using Hadal.AI.TreeNodes;
using System.Collections.Generic;
using Tenshi.UnitySoku;

namespace Hadal.AI.States
{
    public class JudgementSubState : IState
    {
        EngagementState parent;
        AIBrain b;
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

            Test_SetupDefensiveBranchBehaviourTree();
        }

        //! FILO
        private void Test_SetupDefensiveBranchBehaviourTree()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            BTSequence tailWhip = new BTSequence(new List<BTNode>() { new TailWhipNode(b) });
            BTSequence escapeTailWhip = new BTSequence(new List<BTNode>() { tailWhip, setRecoveryState });

            BTSelector hasJT4Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 4) });
            BTSequence recoveryAfterJT4Passed = new BTSequence(new List<BTNode>() { hasJT4Passed, setRecoveryState });

            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b), recoveryAfterJT4Passed });
            BTSequence threshAndRecoveryIfSuccessful = new BTSequence(new List<BTNode>() { threshCarriedPlayer, setRecoveryState });

            BTSelector onePlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 1) });
            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false), threshAndRecoveryIfSuccessful });

            BTSequence sequenceD1 = new BTSequence(new List<BTNode>()
            {
                onePlayerInCavern,
                isCarryingAnyPlayer,
                hasJT4Passed,
                escapeTailWhip
            });

            root = new BTSequence(new List<BTNode>() { sequenceD1 });
        }

        private void Test_SetupOffensiveBranchBehaviourTree1()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });

            BTSequence increaseConfidence = new BTSequence(new List<BTNode>() { new IncreaseConfidenceNode() });

            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false) });
            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b) });
            BTSequence threshNearestTarget = new BTSequence(new List<BTNode>() { isCarryingAnyPlayer, threshCarriedPlayer });

            //Fallback if threshNearestTarget fails
            BTSelector hasJT4Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 4) });
            BTSelector decreaseConfidence = new BTSelector(new List<BTNode>() { new DecreaseConfidenceNode() });
            BTSelector resetCummulativeDamageThreshold = new BTSelector(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b) });

            BTSelector onePlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 1) });

            BTSequence sequenceA1 = new BTSequence(new List<BTNode>()
            {
                onePlayerInCavern,
                threshNearestTarget,
                increaseConfidence
            });

            BTSequence threshFallbackA1 = new BTSequence(new List<BTNode>()
            {
                hasJT4Passed,
                decreaseConfidence,
                resetCummulativeDamageThreshold
            });

            root = new BTSequence(new List<BTNode>() { sequenceA1 });
            root = new BTSequence(new List<BTNode>() { threshFallbackA1 });

        }

        public void OnStateStart() { }
        public void StateTick()
        {
            b.RuntimeData.TickJudgementTimer(b.DeltaTime);
            if (TickUpdateTimer(b.DeltaTime) > updateDelay)
            {
                ResetUpdateTimer();
                var result = root.Evaluate();
                if (result == NodeState.RUNNING) "Tree: Running".Msg();
                else if (result == NodeState.SUCCESS) "Tree: Success".Msg();
                else if (result == NodeState.FAILURE) "Tree: Fail".Msg();
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
