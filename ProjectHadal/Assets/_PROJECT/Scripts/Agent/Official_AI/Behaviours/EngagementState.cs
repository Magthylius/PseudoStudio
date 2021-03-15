using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using System;
using Tenshi.UnitySoku;
using Timer = Hadal.Utility.Timer;
using System.Collections.Generic;
using System.Linq;
using Hadal.Utility;
using Hadal.Networking;
using ExitGames.Client.Photon;

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

        public AggressiveSubState(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
            pinTimer = parent.Brain.Create_A_Timer().WithDuration(10f)
                                                    .WithShouldPersist(true)
                                                    .WithOnCompleteEvent(() => canPin = true)
                                                    .Build()
                                                    .PausedOnStart();
            parent.Brain.AttachTimer(pinTimer);
            canPin = true;
            isPinning = false;
            NetworkEventManager.Instance.AddListener(NetworkEventManager.ByteEvents.AI_PIN_EVENT, Receive_PinTargetPlayer);
        }
        public void OnStateStart()
        {
            parent.SetTargetPlayer(parent.ChooseClosestRandomPlayer());
        }
        public void StateTick()
        {
            parent.ChaseTargetPlayer();
            PinTargetPlayer();
        }
        public void OnStateEnd()
        {
        }
        /// <summary>Detection for wall</summary>
        void SphereObstacleDetection()
        {
            Collider[] results;
            results = Physics.OverlapSphere(b.transform.position, b.wallDetectionRadius, b.obstacleMask);
            $"Result count: {results.Length}".Msg();
            
            var distance = Mathf.Infinity;
            foreach (var points in results)
            {
                $"Walls: {points}".Msg();
                var diff = (b.transform.position - points.transform.position).magnitude;
                if (diff < distance)
                {
                    closestWall = points.transform.position;
                    distance = diff;
                }
            }
        }
        /// <summary>Move to the closest wall found and damage player</summary>
        void MoveToClosestWall()
        {
            SphereObstacleDetection(); 
            b.transform.LookAt(closestWall);
            Vector3 currentVector = (closestWall - b.transform.position).normalized;
            parent.Brain.transform.position += currentVector * (b.pinSpeed * Time.deltaTime);

            if ((closestWall - b.transform.position).magnitude < 50f)
            {
                isPinning = false;
                b.InvokeDamagePlayerEvent(parent.TargetPlayer, AIDamageType.Pin);
                "No longer pinning".Msg();
            }
        }
        /// <summary>Pin the target player to the wall</summary>
        void PinTargetPlayer() 
        {
            //! Check if target player is in range && far from target wall
            if(parent.TargetPlayerIsInRange() && canPin)
            {
                pinTimer.Restart();
                canPin = false;
                isPinning = true;
            }
            
            if (isPinning)
            {
                MoveToClosestWall(); //! Move to closest wall
                if(Vector3.Distance(parent.TargetPlayer.position, b.transform.position + (b.transform.forward * 20f)) < 0.05f)
                {
                    // parent.TargetPlayer.position = b.transform.position + (b.transform.forward * 20f);
                    int viewID = b.GetViewIDMethod(parent.TargetPlayer);
                    Vector3 setPosition = b.transform.position + (b.transform.forward * 20f);

                    object[] data = { isPinning, viewID, setPosition };
                    NetworkEventManager.Instance.RaiseEvent(NetworkEventManager.ByteEvents.AI_PIN_EVENT, data);
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

            //!do i test now? the transform view classic not working right? do we wanna use the transform view only?
            //! it is odd, because transform view classic should work :thinking: .
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
        }
        public void StateTick()
        {
            subStateMachine.MachineTick();
            if (bfightTimer)
            {
                fightTimer += Time.deltaTime;
            }
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
            float speed = 5f;
            var target = TargetPlayer.position - (Brain.transform.forward * 5f);
            Brain.transform.position = Vector3.Lerp(Brain.transform.position, target, speed * Time.deltaTime);
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
                if(hit)
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
