// using System.Collections;
// using Tenshi;
// using Tenshi.UnitySoku;
// using UnityEngine;

// namespace Hadal.AI.TreeNodes
// {
//     public class TailWhipNode : BTNode
//     {
//         private AIBrain _brain;
//         private AITailManager _tailManager;
//         private bool _tailWhipDone;
//         private bool _startTimer;
//         private float _whipTimer;
//         private float _whipTime;

//         public TailWhipNode(AIBrain brain, float whipTime)
//         {
//             _brain = brain;
//             _tailManager = brain.TailManager;
//             _tailWhipDone = false;
//             _startTimer = false;
//             _whipTime = whipTime;
//         }

//         public override NodeState Evaluate(float deltaTime)
//         {
//             if (_startTimer)
//             {
//                 if (TickWhipTimer(_brain.DeltaTime) <= 0)
//                 {
//                     ResetWhipTimer();
//                     _tailWhipDone = true;
//                     _startTimer = false;
//                     Debugger();
//                     Debug.LogWarning("TailWhip:" + _tailWhipDone);
//                 }
//             }
//             else
//             {
//                 _startTimer = true;
//                 _tailWhipDone = false;
//                 _brain.TailManager.Send_ApplyKnockback(_brain.SenseDetection.DetectedPlayers);
//             }

//             if (!_tailWhipDone)
//                 return NodeState.RUNNING;
            
//             return NodeState.SUCCESS;
//         }

//         private float TickWhipTimer(in float deltaTime) => _whipTimer -= deltaTime;
//         private void ResetWhipTimer() => _whipTimer = _whipTime;

//          public TailWhipNode WithDebugName(string msg)
//         {
//             debugName = msg.AddSpacesBeforeCapitalLetters(false) + "Node";
//             return this;
//         }

//         private void Debugger()
//         {
//             if (EnableDebug)
//                 $"Name: {debugName}, TailWhipDone?: {_tailWhipDone}".Msg();
//         }
//     }
// }
