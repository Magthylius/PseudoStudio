using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIContextHandler : MonoBehaviour
    {
        private void Start()
        {
            SetupTorpedoSalvage();
        }
        

        [Header("Revive UI")] 
        [SerializeField] private Animator reviveAnimator;
        private static readonly int PlayerDownHash = Animator.StringToHash("PlayerDown");
        private static readonly int PlayerRevivedHash = Animator.StringToHash("PlayerRevived");
        
        public void PlayerWentDown() => reviveAnimator.SetTrigger(PlayerDownHash);
        public void PlayerRevived() => reviveAnimator.SetTrigger(PlayerRevivedHash);
        
        [Header("Jumpstart UI")]
        [SerializeField] private Animator jumpstartAnimator;
        private static readonly int AllowJumpstart = Animator.StringToHash("AllowJumpstart");
        private static readonly int JumpstartSucceed = Animator.StringToHash("JumpstartSucceed");
        private static readonly int JumpstartFailure = Animator.StringToHash("JumpstartFailure");

        public void StartJumpstart() => jumpstartAnimator.SetTrigger(AllowJumpstart);
        public void SuccessJumpstart()
        {
            jumpstartAnimator.ResetTrigger(AllowJumpstart);
            jumpstartAnimator.SetTrigger(JumpstartSucceed);
        }
        public void FailJumpstart()
        {
            jumpstartAnimator.ResetTrigger(AllowJumpstart);
            jumpstartAnimator.SetTrigger(JumpstartFailure);
        }

        [Header("Torpedo Salvage UI")] 
        [SerializeField] private Image salvageLeftFiller;
        [SerializeField] private Image salvageRightFiller;
        [SerializeField] private Animator salvageAnimator;
        private static readonly int StartSalvage = Animator.StringToHash("StartSalvage");
        private static readonly int SalvageOutcome = Animator.StringToHash("SalvageOutcome");

        private Coroutine torpCoroutine = null;
        
        void SetupTorpedoSalvage()
        {
            salvageLeftFiller.fillAmount = 0f;
            salvageRightFiller.fillAmount = 0f;
        }

        public void StartSalvageFiller(float time)
        {
            //Debug.LogWarning($"UI Start Received");
            torpCoroutine = StartCoroutine(AnimateFillers());
            salvageLeftFiller.gameObject.SetActive(true);
            salvageRightFiller.gameObject.SetActive(true);
            
            if (!salvageAnimator.isInitialized) salvageAnimator.Rebind();
            salvageAnimator.SetTrigger(StartSalvage);
            
            IEnumerator AnimateFillers()
            {
                while (salvageLeftFiller.fillAmount < 1f)
                {
                    float stepsNeeded = Time.deltaTime / time;
                    salvageLeftFiller.fillAmount += stepsNeeded;
                    salvageRightFiller.fillAmount += stepsNeeded;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public void EndSalvageFiller(bool success)
        {
            if (torpCoroutine != null) StopCoroutine(torpCoroutine);
            
            //Debug.LogWarning($"UI End Received");
            salvageAnimator.ResetTrigger(StartSalvage);
            if (success) salvageAnimator.SetFloat(SalvageOutcome, 1f);
            else salvageAnimator.SetFloat(SalvageOutcome, -1f);

            salvageLeftFiller.fillAmount = 0f;
            salvageRightFiller.fillAmount = 0f;
        }
    }
}
