using System.Collections;
using NaughtyAttributes;
using Tenshi.UnitySoku;
using UnityEditor.Animations;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugAIBlendTreeAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorController controller;
        [SerializeField] private Vector2 target;
        [SerializeField] private float lerpTime;
        [SerializeField] private AnimationClip carryClip;
        [SerializeField] private string animXString;
        [SerializeField] private string animYString;
        [SerializeField] private BlendTree animationTree;

        [SerializeField] private Vector2 xYOnStart;

        private void Awake()
        {
            if (animator == null)
            {
                "No animator has been assigned yet.".Warn();
                return;
            }
            animator.SetFloat("Blend", 1f);
            animator.SetFloat(animXString, xYOnStart.x);
            animator.SetFloat(animYString, xYOnStart.y);
        }

        [Button("Go Swim", EButtonEnableMode.Playmode)]
        private void SetAnimationSwim()
        {
            animator.SetTrigger("Swim");
            animator.SetFloat("Blend", 1f);
            animator.SetFloat("SpeedMultiplier", 1f);
        }
        [Button("Go Carry", EButtonEnableMode.Playmode)]
        private void SetAnimationCarry()
        {
            animator.SetTrigger("Carry");
        }

        [Button("Start Animation Lerp", EButtonEnableMode.Playmode)]
        private void StartAnimationLerp()
        {
            if (animator == null)
            {
                "No animator has been assigned yet.".Warn();
                return;
            }
            StopAllCoroutines();
            StartCoroutine(LerpAnimation());
        }

        private IEnumerator LerpAnimation()
        {
            Vector2 currentVec = new Vector2(animator.GetFloat(animXString), animator.GetFloat(animYString));
            Vector2 target = this.target;
            float lerpTime = this.lerpTime;
            float percent = 0f;

            StartCoroutine(StopSpeedAfterDelay());
            animator.Play("Tree", 0, 0f);

            while (percent < 1f)
            {
                percent += Time.deltaTime * lerpTime;

                currentVec = Vector2.Lerp(currentVec, target, percent);
                animator.SetFloat("Blend", Mathf.Lerp(animator.GetFloat("Blend"), 0f, percent));
                animator.SetFloat(animXString, currentVec.x);
                animator.SetFloat(animYString, currentVec.y);
                yield return null;
            }

            var root = controller.layers[0].stateMachine;
            var stateWithBlendTree = root.states[0].state;
            var blendTree = (BlendTree)stateWithBlendTree.motion;

            var pos = blendTree.children[0].position;

            // info[1].clip
            // const int BaseLayerIndex = 0;
            // var stateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
            // animator.Play("Swimming Blend Tree", -1, 0);
        }

        private IEnumerator StopSpeedAfterDelay()
        {
            float time = carryClip.length;
            yield return new WaitForSeconds(time);

            // animator.SetFloat("SpeedMultiplier", 0f);
        }
    }
}
