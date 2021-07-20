using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugAIBlendTreeAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float lerpTime;
        [SerializeField] private List<AnimationFloat> floats;
        [SerializeField] private string target;

        private enum Typo
        {
            Swim = 0,
            Carry
        }

        private void Awake()
        {
            if (animator == null)
            {
                "No animator has been assigned yet.".Warn();
                return;
            }

            ResetSpeed();
            floats.ForEach(f => f.Initialise(animator));
        }

        [Button("Go to Target Float", EButtonEnableMode.Playmode)]
        private void SetAnimationTarget()
        {
            StartAnimationLerp(target);
        }

        private void StartAnimationLerp(string targetName)
        {
            if (animator == null)
            {
                "No animator has been assigned yet.".Warn();
                return;
            }
            StopAllCoroutines();
            StartCoroutine(LerpAnimation(targetName));
        }

        private IEnumerator LerpAnimation(AnimationFloat animationFloat)
        {
            yield return StartCoroutine(LerpAnimation(animationFloat.GetName()));
        }

        private IEnumerator LerpAnimation(string targetName)
        {
            AnimationFloat currentAnimFloat = floats.Where(f => f.GetName() == targetName).Single();
            List<AnimationFloat> otherFloats = floats.Where(f => f != currentAnimFloat).ToList();
            
            float lerpTime = this.lerpTime;
            float percent = 0f;

            ResetSpeed();
            if (currentAnimFloat.ShouldPauseOnClipFinished())
                StartCoroutine(StopSpeedAfterDelay(currentAnimFloat.GetClipLength()));

            if (currentAnimFloat.ShouldRefreshTree())
                RefreshBlendTree();
            
            if (currentAnimFloat.GetExitName() != string.Empty)
                StartCoroutine(PlayAfterDelay(currentAnimFloat.GetClipLength() + currentAnimFloat.GetExitTime(), currentAnimFloat.GetExitName()));

            while (percent < 1f)
            {
                percent += Time.deltaTime * lerpTime;

                currentAnimFloat.LerpToFocusedValue(percent);
                otherFloats.ForEach(f => f.LerpToUnfocusedValue(percent));
                yield return null;
            }
            
            yield break;
        }

        private void RefreshBlendTree()
        {
            if (animator == null) return;
            animator.Play("Tree", 0, 0f);
        }

        private IEnumerator PlayAfterDelay(float delayInSeconds, string targetName)
        {
            yield return new WaitForSeconds(delayInSeconds);
            StartCoroutine(LerpAnimation(targetName));
        }

        private IEnumerator StopSpeedAfterDelay(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            StopSpeed();
        }

        private void StopSpeed() => animator.SetFloat("SpeedMultiplier", 0f);
        private void ResetSpeed() => animator.SetFloat("SpeedMultiplier", 1f);

        [System.Serializable]
        public class AnimationFloat
        {
            [SerializeField] private string name;
            [Range(0f, 1f), SerializeField] private float defaultValue;
            [Range(0f, 1f), SerializeField] private float focusedValue = 1f;
            [Range(0f, 1f), SerializeField] private float unfocusedValue = 0f;
            [SerializeField] private bool pauseOnClipFinished;
            [SerializeField] private bool refreshTreeOnClipStart;
            [SerializeField] private AnimationClip associatedClip;
            [SerializeField] private string exitToName = string.Empty;
            [SerializeField] private float exitAfterXSeconds = 2f;

            private Animator anim = null;

            public AnimationFloat LerpToFocusedValue(float percent)
            {
                float result = Mathf.Lerp(GetValue(), GetFocusedValue(), percent);
                SetValue(result);
                return this;
            }

            public AnimationFloat LerpToUnfocusedValue(float percent)
            {
                float result = Mathf.Lerp(GetValue(), GetUnfocusedValue(), percent);
                SetValue(result);
                return this;
            }
            
            public string GetName() => name;
            public float GetValue() => anim.GetFloat(name);
            public void SetValue(float value) => anim.SetFloat(name, value);
            public float GetDefaultValue() => defaultValue;
            public float GetFocusedValue() => focusedValue;
            public float GetUnfocusedValue() => unfocusedValue;
            public bool ShouldPauseOnClipFinished() => pauseOnClipFinished;
            public bool ShouldRefreshTree() => refreshTreeOnClipStart;
            public float GetClipLength() => associatedClip.length;
            public string GetExitName() => exitToName;
            public float GetExitTime() => exitAfterXSeconds;
            public void Initialise(Animator animator)
            {
                anim = animator;
                SetValue(GetDefaultValue());
            }
        }
    }
}
