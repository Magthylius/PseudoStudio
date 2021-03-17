using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using System;
using Tenshi.UnitySoku;
using Timer = Hadal.Utility.Timer;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Hadal.Utility;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Debug = UnityEngine.Debug;
using Photon.Realtime;

namespace Hadal.AI.States
{
    public class AggressiveSubState : IState
    {
        EngagementState parent;
        Vector3 closestWall;
        AIBrain b;
        Timer pinTimer;
        bool canPin;
        bool isPinning;
        bool foundWall;

        public AggressiveSubState(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
            pinTimer = parent.Brain.Create_A_Timer().WithDuration(40f)
                                                    .WithShouldPersist(true)
                                                    .WithOnCompleteEvent(() => canPin = true);
            pinTimer.Pause();
            canPin = true;
            isPinning = false;
            foundWall = false;
            NetworkEventManager.Instance.AddListener(ByteEvents.AI_PIN_EVENT, Receive_PinTargetPlayer);
        }
        public void OnStateStart()
        {

        }
        public void StateTick()
        {
            if (parent.TargetPlayer == null)
                parent.SetTargetPlayer(parent.ChooseClosestRandomPlayer());

            PinTargetPlayer();
        }
        public void LateStateTick()
        {
            foundWall = false;
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd()
        {
            
        }
        /// <summary>Detection for wall</summary>
        void SphereObstacleDetection()
        {
            // Check for which wall is closest
            Collider[] results;
            results = Physics.OverlapSphere(b.transform.position, b.wallDetectionRadius, b.obstacleMask);

            var distance = Mathf.Infinity;
            Transform closestTrans = null;
            foreach (var points in results)
            {
                var diff = (b.transform.position - points.transform.position).magnitude;
                if (diff < distance)
                {
                    closestTrans = points.transform;
                    closestWall = points.transform.position;
                    distance = diff;
                    foundWall = true;
                }
            }

            // Get the surface of the chosen wall
            if (closestTrans == null) return;
            var dir = closestTrans.position - b.transform.position;
            if (Physics.Raycast(b.transform.position, dir, out var hit, Mathf.Infinity, b.obstacleMask))
            {
                closestWall = hit.point;
                foundWall = true;
            }
        }

        /// <summary>Move to the closest wall found and damage player</summary>
        void MoveToClosestWall()
        {
            //b.transform.LookAt(closestWall);
            // b.transform.position = Vector3.Lerp(b.transform.position, closestWall, b.pinSpeed * Time.deltaTime);
            // Vector3 tempVect = (closestWall - b.transform.position);

            Vector3 velo = closestWall - b.transform.position;
            velo = velo.normalized;
            velo *= (b.pinSpeed);
            b.rb.AddRelativeForce(velo, ForceMode.VelocityChange);
            b.transform.LookAt(closestWall);

            float speed = Vector3.Magnitude (b.rb.velocity);  // test current object speed
      
            if (speed > 5f)
            
            {
                float brakeSpeed = speed - 10f;  // calculate the speed decrease
            
                Vector3 normalisedVelocity = b.rb.velocity.normalized;
                Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;  // make the brake Vector3 value
            
               b.rb.AddForce(-brakeVelocity);  // apply opposing brake force
            }

             if(b.rb.velocity.sqrMagnitude > 5f)
             {
                 //smoothness of the slowdown is controlled by the 0.99f, 
                 //0.5f is less smooth, 0.9999f is more smooth
                     b.rb.velocity *= 0.99f;
             }

            // b.transform.position = closestWall;
            // tempVect = tempVect * b.pinSpeed * Time.deltaTime;
            // b.rb.MovePosition(b.transform.position + tempVect);

            // tempVect *= 100000000000f;
            // b.rb.AddForce(tempVect, ForceMode.Force);
            // b.rb.AddForceAtPosition(tempVect, b.transform.position, ForceMode.Force);

            if (Vector3.Distance(closestWall, b.transform.position) < 15f)
            {
                isPinning = false;
                b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, false);
                //TODO : Another way of setting parent null, this is just a hotfix
                // b.transform.GetChild(1).SetParent(null);
                // b.transform.GetChild(1).parent = null;
                // if(b.transform.GetChild(1).parent == null)
                //     return;
                var child = b.transform.Find("Player(Clone)");
                child.SetParent(null);
                child.parent = null;
            }
        }

        /// <summary>Pin the target player to the wall</summary>
        void PinTargetPlayer()
        {
            //! Check if target player is in range && far from target wall
            if (parent.TargetPlayerIsInRange() && canPin)
            {
                SphereObstacleDetection();
                if (foundWall)
                {
                    b.InvokeDamagePlayerEvent(parent.TargetPlayer, AIDamageType.Pin);
                    pinTimer.Restart();
                    canPin = false;
                    isPinning = true;
                    b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, true);
                }

                // b.InvokeForceSlamPlayerEvent(parent.TargetPlayer, closestWall);
            }

            if (isPinning)
            {
                b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, true);
                MoveToClosestWall();
                parent.ChaseTargetPlayer();

                float distanceBetween = Vector3.Distance(parent.TargetPlayer.position, b.transform.position + (b.transform.forward * 20f));
                if (distanceBetween < 30f)
                {
                    var p = parent.TargetPlayer;

                    Vector3 pleaseMoveHere = b.transform.position + (b.transform.forward * 30f);
                    p.position = pleaseMoveHere;

                    if (p.parent == null) p.SetParent(b.transform);

                    int viewID = b.GetViewIDMethod(parent.TargetPlayer);

                    object[] data = { isPinning, viewID, pleaseMoveHere };
                    RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_PIN_EVENT, data, options);
                }
            }
        }

        private void Receive_PinTargetPlayer(EventData eventData)
        {
            object[] data = eventData.CustomData.AsObjArray();
            if (data == null) return;

            bool shouldPin = data[0].AsBool();
            int viewID = data[1].AsInt();
            Vector3 setPosition = data[2].AsVector3();

            //TODO: we probably need to make an information class in the other assembly, damagemanager is doing everything at the moment
            Transform t = b.playerTransforms.Where(p => b.ViewIDBelongsToTransMethod(p, viewID)).SingleOrDefault();
            if (t == null) return;
            t.position = setPosition;
            $"pin position set to {t.position}".Msg();
        }

        public Func<bool> ShouldTerminate() => () => false;
    }
    public class AmbushSubState : IState
    {
        public void OnStateStart()
        {
        }
        public void StateTick()
        {
        }
        public void LateStateTick()
        {
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd()
        {
        }
        public Func<bool> ShouldTerminate() => () => false;
    }
    public class JudgementSubState : IState
    {
        public void OnStateStart()
        {
        }
        public void StateTick()
        {
        }
        public void LateStateTick()
        {
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd()
        {
        }
        public Func<bool> ShouldTerminate() => () => false;
    }

    public class EngagementState : IState
    {
        public AIBrain Brain { get; private set; }
        StateMachine subStateMachine;
        SubStateState subStateState;

        // Time elapsed after fight start
        float fightTimer = 0f;

        // To start fight timer
        bool bfightTimer;

        public EngagementState(AIBrain brain)
        {
            this.Brain = brain;
            subStateState = SubStateState.Aggresive;
            TargetPlayer = null;

            //! intialise sub machine and states
            subStateMachine = new StateMachine();
            var aggressive = new AggressiveSubState(this);
            var ambush = new AmbushSubState();
            var judgement = new JudgementSubState();

            subStateMachine.AddSequentialTransition(from: aggressive, to: ambush, withCondition: OnAmbush());
            subStateMachine.AddSequentialTransition(from: ambush, to: judgement, withCondition: OnJudgement());
            subStateMachine.AddSequentialTransition(from: judgement, to: aggressive, withCondition: OnAggressive());

            subStateMachine.SetState(aggressive); // default state

            //! transition conditions
            Func<bool> OnAmbush() => () => subStateState is SubStateState.Ambush;
            Func<bool> OnJudgement() => () => subStateState is SubStateState.Judgement;
            Func<bool> OnAggressive() => () => subStateState is SubStateState.Aggresive;
        }
        public void OnStateStart()
        {
            bfightTimer = true;
            subStateMachine.CurrentState.OnStateStart();
        }
        public void StateTick()
        {
            subStateMachine.MachineTick();
            if (bfightTimer)
            {
                fightTimer += Time.deltaTime;
            }
        }
        public void LateStateTick()
        {
        }
        public void FixedStateTick()
        {
        }
        public void OnStateEnd()
        {
            fightTimer = 0f;
        }

        Vector3 curDestination;
        public Transform TargetPlayer { get; private set; }
        Vector3 prevDest;
        bool isFirstPath;
        bool isGridInitialised;

        /// <summary>
        /// Set the target player once detected in AI's sphere range(its senses)
        /// </summary>
        /// <param name="player">The transform of the player to target.</param>
        internal void SetTargetPlayer(Transform player)
        {
            if (player == null) return;
            TargetPlayer = player;
        }

        internal Transform ChooseClosestRandomPlayer()
        {
            if (Brain == null) return null;
            List<Transform> targets = Brain.playerTransforms
                            .Where(p => Vector3.Distance(Brain.transform.position, p.position) < Brain.detectionRadius)
                            .ToList();
            if (targets.IsNullOrEmpty())
                return null;
            return targets.RandomElement();
        }

        internal void ChaseTargetPlayer()
        {
            if (TargetPlayer == null) return;
            var target = TargetPlayer.position - (Brain.transform.forward * 5f);
            Brain.transform.LookAt(target);
            Brain.transform.position = Vector3.Lerp(Brain.transform.position, target, Brain.pinSpeed * Time.deltaTime);
        }

        internal bool TargetPlayerIsInRange()
        {
            if (TargetPlayer == null) return false;

            float dist = Vector3.Distance(Brain.transform.position, TargetPlayer.position);
            if (dist < Brain.detectionRadius)
            {
                //! Detect if the player is not behind any obstacle.
                Vector3 dir = (TargetPlayer.position - Brain.transform.position).normalized;
                //TODO: Change the transform to head transform later on

                Ray ray = new Ray();
                ray.origin = Brain.transform.position;
                if (dist == 0)
                {
                    ray.direction = Brain.transform.forward;
                }
                else
                {
                    ray.direction = dir;
                }

                Debug.DrawLine(ray.origin, ray.direction * 100000f, Color.red);
                bool hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, Brain.playerMask, QueryTriggerInteraction.Collide);
                if (hit)
                {
                    if (hitInfo.transform.gameObject.layer == Brain.playerMask.ToLayer())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;

            // if (!targetPlayer)
            // {
            //     Collider[] objects = Physics.OverlapSphere(Brain.transform.position, Brain.detectionRadius, Brain.playerMask);
            //     if (objects.ToList().Any(t => t == targetPlayer))
            //     {
            //         return true;
            //     }
            // }
            // return false;
        }

        public Func<bool> ShouldTerminate() => () => false;

        enum SubStateState
        {
            Aggresive,
            Ambush,
            Judgement,
            Total
        }
    }
}
