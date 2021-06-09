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
        EngagementStateSettings engagementStateSettings;
        BTSequence rootAgg;
        BTSequence rootDef;
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
            //!Defensive
            SetupDefensiveBranchBehaviourTree();
            
            //!Offensive
            SetupOffensiveBranchBehaviourTree1();
            SetupOffensiveBranchBehaviourTree2();
            SetupOffensiveBranchBehaviourTree3();
            SetupOffensiveBranchBehaviourTree4();
            rootAgg.SetDebugName("RootAgg");
        }


        //! FILO
        private void SetupDefensiveBranchBehaviourTree()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence tailWhip = new BTSequence(new List<BTNode>() { new TailWhipNode(b, 1f) });
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

            rootDef = new BTSequence(new List<BTNode>() { sequenceD1 });
        }

#region Offensive Branch
        private void SetupOffensiveBranchBehaviourTree1()
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

            BTSequence resetCumulativeDamageThreshold = new BTSequence(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b, engagementStateSettings) });
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

            rootAgg.AddNode(sequenceA1);
        }

        private void SetupOffensiveBranchBehaviourTree2()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence increaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, true) });
            increaseConfidence.SetDebugName("increase confidence");

            //Fallback if threshNearestTarget fails
            BTSelector hasJT3Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 3) });
            hasJT3Passed.SetDebugName("has jt3 passed?");

            BTSequence decreaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, false) });
            decreaseConfidence.SetDebugName("decrease confidence");

            BTSequence resetCumulativeDamageThreshold = new BTSequence(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b, engagementStateSettings) });
            resetCumulativeDamageThreshold.SetDebugName("reset cumulative damage threshold");

            BTSequence threshFallbackA2 = new BTSequence(new List<BTNode>()
            {
                hasJT3Passed,
                decreaseConfidence,
                setRecoveryState
            });
            threshFallbackA2.SetDebugName("thresh fallback A1");

            BTSequence getPlayerToCarry = new BTSequence(new List<BTNode>() { new MoveToPlayerNode(b, b.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(b, 1.5f, 0.5f) });
            getPlayerToCarry.SetDebugName("get player to carry");

            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false), getPlayerToCarry });
            isCarryingAnyPlayer.SetDebugName("is carrying any player?");

            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b, damageManager), threshFallbackA2 });
            threshCarriedPlayer.SetDebugName("thresh carried player?");

            BTSequence threshNearestTarget = new BTSequence(new List<BTNode>() { isCarryingAnyPlayer, threshCarriedPlayer });
            threshNearestTarget.SetDebugName("thresh nearest target");

            BTSelector twoPlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 2) });
            twoPlayerInCavern.SetDebugName("two player in cavern?");

            BTSequence postSequenceA2 = new BTSequence(new List<BTNode>() { increaseConfidence, resetCumulativeDamageThreshold });
            postSequenceA2.SetDebugName("post sequence A2");

            BTSequence sequenceA2 = new BTSequence(new List<BTNode>()
            {
                twoPlayerInCavern,
                threshNearestTarget,
                postSequenceA2
            });
            sequenceA2.SetDebugName("sequence A2");

            rootAgg.AddNode(sequenceA2);
        }

        private void SetupOffensiveBranchBehaviourTree3()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence increaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, true) });
            increaseConfidence.SetDebugName("increase confidence");

            //Fallback if threshNearestTarget fails
            BTSelector hasJT2Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 2) });
            hasJT2Passed.SetDebugName("has jt2 passed?");

            BTSequence decreaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, false) });
            decreaseConfidence.SetDebugName("decrease confidence");

            BTSequence resetCumulativeDamageThreshold = new BTSequence(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b, engagementStateSettings) });
            resetCumulativeDamageThreshold.SetDebugName("reset cumulative damage threshold");

            BTSequence threshFallbackA3 = new BTSequence(new List<BTNode>()
            {
                hasJT2Passed,
                decreaseConfidence,
                setRecoveryState
            });
            threshFallbackA3.SetDebugName("thresh fallback A1");

            BTSequence getPlayerToCarry = new BTSequence(new List<BTNode>() { new MoveToPlayerNode(b, b.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(b, 1.5f, 0.5f) });
            getPlayerToCarry.SetDebugName("get player to carry");

            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false), getPlayerToCarry });
            isCarryingAnyPlayer.SetDebugName("is carrying any player?");

            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b, damageManager), threshFallbackA3 });
            threshCarriedPlayer.SetDebugName("thresh carried player?");

            BTSequence threshNearestTarget = new BTSequence(new List<BTNode>() { isCarryingAnyPlayer, threshCarriedPlayer });
            threshNearestTarget.SetDebugName("thresh nearest target");

            BTSelector threePlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 3) });
            threePlayerInCavern.SetDebugName("three player in cavern?");

            BTSequence postSequenceA2 = new BTSequence(new List<BTNode>() { increaseConfidence, resetCumulativeDamageThreshold });
            postSequenceA2.SetDebugName("post sequence A2");

            BTSequence sequenceA3 = new BTSequence(new List<BTNode>()
            {
                threePlayerInCavern,
                threshNearestTarget,
                postSequenceA2
            });
            sequenceA3.SetDebugName("sequence A3");

            rootAgg.AddNode(sequenceA3);
        }

        private void SetupOffensiveBranchBehaviourTree4()
        {
            BTSequence setRecoveryState = new BTSequence(new List<BTNode>() { new ChangeStateNode(b, MainObjective.Recover) });
            setRecoveryState.SetDebugName("set recovery state");

            BTSequence increaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, true) });
            increaseConfidence.SetDebugName("increase confidence");

            //Fallback if threshNearestTarget fails
            BTSelector hasJT1Passed = new BTSelector(new List<BTNode>() { new HasJudgementThresholdExceededNode(b, 1) });
            hasJT1Passed.SetDebugName("has jt1 passed?");

            BTSequence decreaseConfidence = new BTSequence(new List<BTNode>() { new ModifyConfidenceNode(b, 1, false) });
            decreaseConfidence.SetDebugName("decrease confidence");

            BTSequence resetCumulativeDamageThreshold = new BTSequence(new List<BTNode>() { new ResetCumulatedDamageThresholdNode(b, engagementStateSettings) });
            resetCumulativeDamageThreshold.SetDebugName("reset cumulative damage threshold");

            BTSequence threshFallbackA4 = new BTSequence(new List<BTNode>()
            {
                hasJT1Passed,
                decreaseConfidence,
                setRecoveryState
            });
            threshFallbackA4.SetDebugName("thresh fallback A1");

            BTSequence getPlayerToCarry = new BTSequence(new List<BTNode>() { new MoveToPlayerNode(b, b.RuntimeData.navPointPrefab, 2, 1000, false), new CarryTargetNode(b, 1.5f, 0.5f) });
            getPlayerToCarry.SetDebugName("get player to carry");

            BTSelector isCarryingAnyPlayer = new BTSelector(new List<BTNode>() { new IsCarryingAPlayerNode(b, false), getPlayerToCarry });
            isCarryingAnyPlayer.SetDebugName("is carrying any player?");

            BTSelector threshCarriedPlayer = new BTSelector(new List<BTNode>() { new ThreshCarriedPlayerNode(b, damageManager), threshFallbackA4 });
            threshCarriedPlayer.SetDebugName("thresh carried player?");

            BTSequence threshNearestTarget = new BTSequence(new List<BTNode>() { isCarryingAnyPlayer, threshCarriedPlayer });
            threshNearestTarget.SetDebugName("thresh nearest target");

            BTSelector fourPlayerInCavern = new BTSelector(new List<BTNode>() { new IsPlayersInCavernEqualToNode(b, 4) });
            fourPlayerInCavern.SetDebugName("three player in cavern?");

            BTSequence postSequenceA4 = new BTSequence(new List<BTNode>() { increaseConfidence, resetCumulativeDamageThreshold });
            postSequenceA4.SetDebugName("post sequence A4");

            BTSequence sequenceA4 = new BTSequence(new List<BTNode>()
            {
                fourPlayerInCavern,
                threshNearestTarget,
                postSequenceA4
            });
            sequenceA4.SetDebugName("sequence A4");

            rootAgg.AddNode(sequenceA4);
        }
#endregion

        //TODO: Set a way to determine which root to go or probablity?
        int randomNumber;
        NodeState result;
        void RandomizeAggOrDefRoot()
        {
            randomNumber = UnityEngine.Random.Range(0, 1);
        }
        public override void OnStateStart()
        {
            if (b.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
            b.RuntimeData.ResetEngagementTicker();
            RandomizeAggOrDefRoot();
        }
        public override void StateTick()
        {
            //return;
            float deltaTime = b.DeltaTime;
            b.RuntimeData.TickEngagementTicker(deltaTime);
            if (TickUpdateTimer(deltaTime) > updateDelay)
            {
                ResetUpdateTimer();

                if (randomNumber == 0)
                    result = rootAgg.Evaluate(deltaTime);
                else
                    result = rootDef.Evaluate(deltaTime);

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
        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd() { }
        public override Func<bool> ShouldTerminate() => () => false;

        private void ResetUpdateTimer() => updateTimer = 0.0f;
        private float TickUpdateTimer(in float tick) => updateTimer += tick;
    }
}
