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
        [SerializeField] private GameObject reviveDown;
        [SerializeField] private GameObject reviveUp;

        private static readonly int PlayerDownHash = Animator.StringToHash("PlayerDown");
        private static readonly int PlayerRevivedHash = Animator.StringToHash("PlayerRevived");

        private void Start()
        {
            reviveDown.SetActive(false);
            reviveUp.SetActive(false);
        }

        public void PlayerWentDown()
        {
            reviveAnimator.SetTrigger(PlayerDownHash);
            //reviveDown.SetActive(true);
            //reviveUp.SetActive(false);
        }
        
        public void PlayerRevived()
        {
            print("revived!");
            reviveAnimator.SetTrigger(PlayerRevivedHash);
            //reviveDown.SetActive(false);
            //reviveUp.SetActive(true);
        }
    }
}
