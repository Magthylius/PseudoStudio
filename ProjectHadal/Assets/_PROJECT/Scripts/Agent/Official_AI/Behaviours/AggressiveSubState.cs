using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using System;
using Tenshi.UnitySoku;
using Timer = Hadal.Utility.Timer;
using System.Linq;
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
        bool foundWall;

        public AggressiveSubState()
        {
            // pinTimer = parent.Brain.Create_A_Timer().WithDuration(40f)
            //                                         .WithShouldPersist(true)
            //                                         .WithOnCompleteEvent(() => canPin = true);
            // pinTimer.Pause();
            canPin = true;
            isPinning = false;
            foundWall = false;
            // NetworkEventManager.Instance.AddListener(ByteEvents.AI_PIN_EVENT, Receive_PinTargetPlayer);
        }
        public void SetParent(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
        }
        public void OnStateStart()
        {

        }
        public void StateTick()
        {
            if (parent.TargetPlayer == null)
            {
                //! Based on design chart, AI has a list of possible randoms to choose a player from
                // parent.SetTargetPlayer(parent.ChooseClosestRandomPlayer());
            }

            //! cannot use PinTargetPlayer anymore in update
            // PinTargetPlayer();

            //! new logic
            /*
            if (parent.TargetPlayer is not within range)
            {
                path towards parent.TargetPlayer
            }

            if (distance between parent.TargetPlayer and b.transform.position < biteDistThreshold)
            {
                bite parent.TargetPlayer;
                parent.TargetPlayer will follow the mouth of the AI;
            }

            if (parent.TargetPlayer is in mouth)
            {
                thresh parent.TargetPlayer in mouth until interupted or player unalive;
                if (thresh is interupted before parent.TargetPlayer unalives)
                {
                    confidence--
                    goto -> Judgement substate and evaluate normally
                }
                else if (thresh ends up killing parent.TargetPlayer in mouth)
                {
                    confidence++
                    goto -> Judgement substate and evaluate 'Aggressive' branch
                }
            }
            */
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

        /// <summary>Detection for wall</summary>
        void SphereObstacleDetection()
        {
            //! use NavigationHandler's obstacle detection, but we do not need wall pinning anymore
        }

        /// <summary>Move to the closest wall found and damage player</summary>
        void MoveToClosestWall()
        {
            //b.transform.LookAt(closestWall);
            // b.transform.position = Vector3.Lerp(b.transform.position, closestWall, b.pinSpeed * Time.deltaTime);
            // Vector3 tempVect = (closestWall - b.transform.position);

            // Vector3 velo = closestWall - b.transform.position;
            // velo = velo.normalized;
            // velo *= (b.pinSpeed);
            // b.rb.AddRelativeForce(velo, ForceMode.VelocityChange);
            // b.transform.LookAt(closestWall);

            // float speed = Vector3.Magnitude(b.rb.velocity);  // test current object speed

            // if (speed > 5f)

            // {
            //     float brakeSpeed = speed - 10f;  // calculate the speed decrease

            //     Vector3 normalisedVelocity = b.rb.velocity.normalized;
            //     Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;  // make the brake Vector3 value

            //     b.rb.AddForce(-brakeVelocity);  // apply opposing brake force
            // }

            // if (b.rb.velocity.sqrMagnitude > 5f)
            // {
            //     //smoothness of the slowdown is controlled by the 0.99f, 
            //     //0.5f is less smooth, 0.9999f is more smooth
            //     b.rb.velocity *= 0.99f;
            // }

            // b.transform.position = closestWall;
            // tempVect = tempVect * b.pinSpeed * Time.deltaTime;
            // b.rb.MovePosition(b.transform.position + tempVect);

            // tempVect *= 100000000000f;
            // b.rb.AddForce(tempVect, ForceMode.Force);
            // b.rb.AddForceAtPosition(tempVect, b.transform.position, ForceMode.Force);

            // if (Vector3.Distance(closestWall, b.transform.position) < 15f)
            // {
            //     isPinning = false;

            //     $"Before unparent: player's parent is null {parent.TargetPlayer.parent == null}".Msg();

            //     parent.TargetPlayer.SetParent(null);
            //     parent.TargetPlayer.parent = null;

            //     $"After unparent: player's parent is null {parent.TargetPlayer.parent == null}".Msg();

            //     b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, false);
                
            // }
        }

        /// <summary>Convert this to thresh target player method</summary>
        void PinTargetPlayer()
        {
            //! Check if target player is in range && far from target wall
            // if (parent.TargetPlayerIsInRange() && canPin)
            // {
            //     SphereObstacleDetection();
            //     if (foundWall)
            //     {
            //         b.InvokeDamagePlayerEvent(parent.TargetPlayer, AIDamageType.Thresh);
            //         pinTimer.Restart();
            //         canPin = false;
            //         isPinning = true;
            //         b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, true);
            //     }

            //     // b.InvokeForceSlamPlayerEvent(parent.TargetPlayer, closestWall);
            // }

            // if (isPinning)
            // {
            //     b.InvokeFreezePlayerMovementEvent(parent.TargetPlayer, true);
                
            //     float distanceBetween = Vector3.Distance(parent.TargetPlayer.position, b.transform.position + (b.transform.forward * 20f));
            //     if (distanceBetween < 30f)
            //     {
            //         var p = parent.TargetPlayer;

            //         Vector3 pleaseMoveHere = b.transform.position + (b.transform.forward * 30f);
            //         p.position = pleaseMoveHere;

            //         if (p.parent == null) p.SetParent(b.transform);

            //         int viewID = b.GetViewIDMethod(parent.TargetPlayer);

            //         object[] data = { isPinning, viewID, pleaseMoveHere };
            //         RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            //         NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_PIN_EVENT, data, options);
            //     }

            //     MoveToClosestWall();
            //     parent.ChaseTargetPlayer();
            // }
        }

        private void Receive_PinTargetPlayer(EventData eventData)
        {
            object[] data = eventData.CustomData.AsObjArray();
            if (data == null) return;

            bool shouldPin = data[0].AsBool();
            int viewID = data[1].AsInt();
            Vector3 setPosition = data[2].AsVector3();

            //TODO: we probably need to make an information class in the other assembly, damagemanager is doing everything at the moment
            Transform t = b.PlayerTransforms.Where(p => b.ViewIDBelongsToTransMethod(p, viewID)).SingleOrDefault();
            if (t == null) return;
            t.position = setPosition;
            $"pin position set to {t.position}".Msg();
        }

        public Func<bool> ShouldTerminate() => () => false;
    }
}
