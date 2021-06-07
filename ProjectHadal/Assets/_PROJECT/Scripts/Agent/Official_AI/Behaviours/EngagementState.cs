using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using System.Collections.Generic;
using System.Linq;
using Hadal.Networking;
using Debug = UnityEngine.Debug;
using Photon.Realtime;

namespace Hadal.AI.States
{
    public class EngagementState : AIStateBase
    {
        private StateMachine subStateMachine;

        public EngagementState(AIBrain brain, AggressiveSubState aggressive, AmbushSubState ambush, JudgementSubState judgement)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
            RuntimeData = Brain.RuntimeData;

            //! intialise sub machine and states
            aggressive.SetParent(this);
			ambush.SetParent(this);
			judgement.Initialise(this);

            subStateMachine = new StateMachine();
            subStateMachine.AddEventTransition(to: ambush, withCondition: OnAmbush());
            subStateMachine.AddEventTransition(to: aggressive, withCondition: OnAggressive());
            subStateMachine.AddEventTransition(to: judgement, withCondition: OnJudgement());
            
            subStateMachine.SetState(judgement); // default state

            //! transition conditions
            Func<bool> OnAmbush() => () => Brain.RuntimeData.GetEngagementObjective == EngagementObjective.Ambush;
            Func<bool> OnAggressive() => () => Brain.RuntimeData.GetEngagementObjective == EngagementObjective.Aggressive;
            Func<bool> OnJudgement() => () => Brain.RuntimeData.GetEngagementObjective == EngagementObjective.Judgement;
        }
        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
        }
        public override void StateTick()
        {
            subStateMachine.MachineTick();
        }
        public override void LateStateTick()
        {
            subStateMachine.LateMachineTick();
        }
        public override void FixedStateTick()
        {
            subStateMachine.FixedMachineTick();
        }
        public override void OnStateEnd()
        {
            
        }

        /// <summary> Set the target player once detected in AI's sphere range(its senses) </summary>
        /// <param name="player">The transform of the player to target.</param>
        internal void SetTargetPlayer(Transform player)
        {
            
        }

        internal Transform ChooseClosestRandomPlayer(float range)
        {
            if (Brain == null) return null;
            List<Transform> targets = Brain.Players
                            .Select(p => p.GetTarget)
                            .Where(p => Vector3.Distance(Brain.transform.position, p.position) < range)
                            .ToList();
            if (targets.IsNullOrEmpty())
                return null;
            return targets.RandomElement();
        }

        internal void ChaseTargetPlayer()
        {
            // if (TargetPlayer == null) return;
            // var target = TargetPlayer.position - (Brain.transform.forward * 5f);
            // Brain.transform.LookAt(target);
            // Brain.transform.position = Vector3.Lerp(Brain.transform.position, target, Brain.pinSpeed * Time.deltaTime);

            //! use NavigationHandler
        }

        internal bool TargetPlayerIsInRange(float range)
        {
			return false;
			/*
            if (TargetPlayer == null) return false;

            float dist = Vector3.Distance(Brain.transform.position, TargetPlayer.position);
            if (dist < range)
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
                bool hit = Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, Brain.RuntimeData.PlayerMask, QueryTriggerInteraction.Collide);
                if (hit)
                {
                    if (hitInfo.transform.gameObject.layer == Brain.RuntimeData.PlayerMask.ToLayer())
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
			*/
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
