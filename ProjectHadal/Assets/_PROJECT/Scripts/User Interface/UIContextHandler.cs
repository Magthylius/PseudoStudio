using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIContextHandler : MonoBehaviour
    {
        private void Start()
        {
            InitReviveUI();
        }

        
        [Header("Revive UI")] 
        [SerializeField] private Animator reviveAnimator;
        [SerializeField] private GameObject reviveDown;
        [SerializeField] private GameObject reviveUp;

        private static readonly int PlayerDownHash = Animator.StringToHash("PlayerDown");
        private static readonly int PlayerRevivedHash = Animator.StringToHash("PlayerRevived");

        void InitReviveUI()
        {
            reviveDown.SetActive(false);
            reviveUp.SetActive(false);
        }

        public void PlayerWentDown()
        {
            reviveAnimator.SetTrigger(PlayerDownHash);
        }
        
        public void PlayerRevived()
        {
            //print("revived!");
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
