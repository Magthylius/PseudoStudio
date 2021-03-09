using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using System;
using System.Linq;
using Hadal.AI.AStarPathfinding;
using Tenshi.UnitySoku;
using Hadal.AI.GeneratorGrid;

namespace Hadal.AI.States
{
    public class AggressiveSubState : IState
    {
        EngagementState parent;
        Vector3 closestWall;
        AIBrain b;

        public AggressiveSubState(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
        }
        public void OnStateStart()
        {
            parent.SetTargetPlayer(parent.ChooseRandomPlayer());
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
                Debug.Log("Walls:" + points); //This does not get called
                var diff = (b.transform.position - points.transform.position).magnitude;
                if (diff < distance)
                {
                    closestWall = points.transform.position;
                    distance = diff;
                }
            }
        }
        /// <summary>Move to the closest wall found</summary>
        void MoveToClosestWall()
        {
            SphereObstacleDetection(); 
            b.transform.LookAt(closestWall);
            Vector3 currentVector = (closestWall - b.transform.position).normalized;
            parent.Brain.transform.position += currentVector * (b.pinSpeed * Time.deltaTime);

            if ((closestWall - b.transform.position).magnitude < 0.3f)
            {
                b.transform.position = closestWall;
            }
        }
        /// <summary>Pin the target player to the wall</summary>
        void PinTargetPlayer() 
        {
            //! Check if target player is in range && far from target wall
            if(parent.TargetPlayerIsInRange())
            {
                Debug.Log("Hi");
                MoveToClosestWall(); //! Move to closest wall
                if(Vector3.Distance(closestWall, b.transform.position) > 0.2f)
                    b.transform.position = parent.TargetPlayer.position; //!Instant teleport player to the wall with AI
                
            }
            TLog.Vector(closestWall, "Closest Wall");
            
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

        internal Transform ChooseRandomPlayer()
        {
            return Brain.playerTransforms.RandomElement();
        }

        internal void ChaseTargetPlayer()
        {
            Vector3 direction = (TargetPlayer.position - Brain.transform.position).normalized;
            float multiplier = (Vector3.Distance(Brain.transform.position, curDestination) + 0.1f);
            Brain.transform.position = Vector3.Lerp(Brain.transform.position, TargetPlayer.position, multiplier * Time.deltaTime);
        }

        internal bool TargetPlayerIsInRange()
        {
            if (TargetPlayer == null) return false;
            1.Msg();
            
            float dist = Vector3.Distance(Brain.transform.position, TargetPlayer.position);
            if (dist < Brain.detectionRadius)
            {
                $"{2}, Target player is there: {TargetPlayer != null}, position is: {TargetPlayer.position}".Msg();
                //! Detect if the player is not behind any obstacle.
                Vector3 dir = (TargetPlayer.position - Brain.transform.position).normalized;
                                    //TODO: Change the transform to head transform later on
                
                Ray ray = new Ray();
                ray.origin = Brain.transform.position;
                ray.direction = dir;

                Debug.DrawLine(ray.origin, ray.direction * 100000f, Color.red);
                if(Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, Brain.playerMask))
                {
                    3.Msg();
                    if (hitInfo.transform.gameObject.layer == Brain.playerMask.ToLayer())
                    {
                        4.Msg();
                        return true;
                    }
                    else
                    {
                        5.Msg();
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
