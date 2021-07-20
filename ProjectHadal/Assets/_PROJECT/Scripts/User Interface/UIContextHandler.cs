using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIContextHandler : MonoBehaviour
    {
        [Header("Revive UI")] 
        [SerializeField] private Animator reviveAnimator;

        private static readonly int PlayerDownHash = Animator.StringToHash("PlayerDown");
        private static readonly int PlayerRevivedHash = Animator.StringToHash("PlayerRevived");
        
        public void PlayerWentDown()
        {
            reviveAnimator.SetTrigger(PlayerDownHash);
        }
        
        public void PlayerRevived()
        {
            reviveAnimator.SetTrigger(PlayerRevivedHash);
        }
        
        [Header("Jumpstart UI")]
        [SerializeField] private Animator jumpstartAnimator;
        private static readonly int AllowJumpstart = Animator.StringToHash("AllowJumpstart");
        private static readonly int JumpstartSucceed = Animator.StringToHash("JumpstartSucceed");
        private static readonly int JumpstartFailure = Animator.StringToHash("JumpstartFailure");

        public void StartJumpstart() => jumpstartAnimator.SetTrigger(AllowJumpstart);
        public void SuccessJumpstart() => jumpstartAnimator.SetTrigger(JumpstartSucceed);
        public void FailJumpstart() => jumpstartAnimator.SetTrigger(JumpstartFailure);
    }
}
