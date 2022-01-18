// using Tenshi.UnitySoku;
// using Hadal.AI.States;

// namespace Hadal.AI.TreeNodes
// {
//     public class ResetCumulatedDamageThresholdNode : BTNode
//     {
//         private AIBrain _brain;
//         EngagementStateSettings _engagementSettings;
//         bool resetDone;

//         public ResetCumulatedDamageThresholdNode(AIBrain brain, EngagementStateSettings engagementSettings)
//         {
//             _brain = brain;
//             _engagementSettings = engagementSettings;
//             resetDone = false;
//         }

//         void ResetEngagementThreshold()
//         {
//             if (!resetDone)
//             {
//                 _brain.RuntimeData.UpdateCumulativeDamageCountThreshold(_engagementSettings.GetAccumulatedDamageThreshold(_brain.HealthManager.GetCurrentHealth));
//                 resetDone = true;
//             }

//         }

//         public override NodeState Evaluate(float deltaTime)
//         {
//             if (resetDone)
//                 return NodeState.SUCCESS;
//             else
//             {
//                 ResetEngagementThreshold();
//                 return NodeState.RUNNING;
//             }

//         }
//     }
// }
