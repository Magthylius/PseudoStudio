using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using NaughtyAttributes;
using System.Linq;
using ReadOnly = Tenshi.ReadOnlyAttribute;
using Tenshi.UnitySoku;

namespace Hadal.AI
{
    public class AIAnimationManager : MonoBehaviour
    {
		[SerializeField] private Animator animator;
		[SerializeField] private float defaultAnimLerpTime;
		[SerializeField] private float stunnedAnimSpeedMultiplier = 0.2f;
		[SerializeField] private List<AnimationFloat> floatData;
		
		private bool isSpeedStopped;
		private bool shouldUseStunMultiplier;
		private AIBrain _brain;
		private Coroutine mainLerpRoutine;
		private Coroutine stopDelaySubroutine;
		
		public void Initialise(AIBrain brain)
		{
			//! Class initialisations
			_brain = brain;
			if (animator == null) animator = GetComponent<Animator>();
			floatData.ForEach(f => f.Initialise(animator));
			
			//! Events
			_brain.OnStunnedEvent += HandleStunEvent;

			//! Default animation state
			SetAnimation(AIAnim.Swim);
		}

		private void OnDestroy()
		{
			if (_brain != null)
				_brain.OnStunnedEvent -= HandleStunEvent;
		}
		
		public void SetAnimation(AIAnim animType, float customAnimLerpTime = -1f)
		{
			StopAllRunningCoroutines();
			mainLerpRoutine = StartCoroutine(LerpAnimation(animType, customAnimLerpTime));
			_brain.Send_SetAnimation(animType, customAnimLerpTime);
		}

		public float GetAnimationClipLengthFor(AIAnim animType) => GetAnimationFloatFromAnimType(animType).GetClipLength();
		
		private IEnumerator LerpAnimation(AIAnim animType, float customAnimLerpTime)
		{
			AnimationFloat currentFloat = GetAnimationFloatFromAnimType(animType);
			List<AnimationFloat> otherFloats = GetAnimationFloatsExcluding(animType);

			if (currentFloat == null)
			{
				string theName = System.Enum.GetName(typeof(AIAnim), animType);
				$"Unable to find animation float of enum value: {theName}, is it not properly assigned?".Warn();
				yield break;
			}

			float lerpTime = 1f / (customAnimLerpTime > 0f ? customAnimLerpTime : defaultAnimLerpTime);
			float percent = 0f;
			
			ResetSpeed();
			if (currentFloat.ShouldPauseOnClipFinished())
			{
				isSpeedStopped = false;
				stopDelaySubroutine = StartCoroutine(StopSpeedAfterDelay(currentFloat.GetClipLength()));
				RefreshBlendTree();
			}

			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpTime;
				currentFloat.LerpToFocusedValue(percent);
				otherFloats.ForEach(f => f.LerpToUnfocusedValue(percent));
				yield return null;
			}
			
			mainLerpRoutine = null;
		}

		private IEnumerator StopSpeedAfterDelay(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            isSpeedStopped = true;
			StopSpeed();
			stopDelaySubroutine = null;
        }

		private bool waitForUnstun = false;
		private void HandleStunEvent(bool isStunned)
		{
			shouldUseStunMultiplier = isStunned;
			UpdateStunnedSpeed();

			if (!isStunned && waitForUnstun)
				SetAnimation(GetDefaultAnimationFloat().GetEnum());

			waitForUnstun = isStunned;
		}

		private void RefreshBlendTree()
        {
            if (animator == null) return;
            animator.Play("Tree", 0, 0f);
        }

		private void StopSpeed() => animator.SetFloat("SpeedMultiplier", 0f);
        private void ResetSpeed() => animator.SetFloat("SpeedMultiplier", 1f * GetStunnedMultiplier());
		private void UpdateStunnedSpeed() => animator.SetFloat("SpeedMultiplier", animator.GetFloat("SpeedMultiplier") * GetStunnedMultiplier());
		private float GetStunnedMultiplier()
		{
			bool IsStunnedAndSpeedIsNotStopped = shouldUseStunMultiplier && !isSpeedStopped;
			if (IsStunnedAndSpeedIsNotStopped)
				return stunnedAnimSpeedMultiplier;

			return 1f;
		}

		private AnimationFloat GetAnimationFloatFromAnimType(AIAnim animType)
		{
			int i = -1;
			while (++i < floatData.Count)
			{
				if (floatData[i].GetEnum() == animType)
					return floatData[i];
			}
			return null;
		}

		private List<AnimationFloat> GetAnimationFloatsExcluding(AIAnim excludeType)
		{
			return floatData.Where(f => f.GetEnum() != excludeType).DefaultIfEmpty().ToList();
		}

		private AnimationFloat GetDefaultAnimationFloat() => floatData.Where(f => f.IsDefault()).Single();
		
		private void StopAllRunningCoroutines()
		{
			if (mainLerpRoutine != null)
				StopCoroutine(mainLerpRoutine);
			mainLerpRoutine = null;

			if (stopDelaySubroutine != null)
				StopCoroutine(stopDelaySubroutine);
			stopDelaySubroutine = null;
		}

		private void OnValidate()
		{
			floatData.ForEach(f => f.RefreshCachedName());
		}

		[System.Serializable]
        private class AnimationFloat
        {
			[SerializeField, ReadOnly] private string name = string.Empty;
            [SerializeField] private AIAnim animationEnum;
            [Range(0f, 1f), SerializeField] private float defaultValue;
            [Range(0f, 1f), SerializeField] private float focusedValue = 1f;
            [Range(0f, 1f), SerializeField] private float unfocusedValue = 0f;
			[SerializeField] private bool isDefaultAnimationClip;
            [SerializeField] private bool pauseOnClipFinished;
            [SerializeField] private AnimationClip associatedClip;
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
            
            public float GetValue() => anim.GetFloat(GetName());
            public void SetValue(float value) => anim.SetFloat(GetName(), value);
            public string GetName() => name;
			public AIAnim GetEnum() => animationEnum;
            public float GetDefaultValue() => defaultValue;
            public float GetFocusedValue() => focusedValue;
            public float GetUnfocusedValue() => unfocusedValue;
			public bool IsDefault() => isDefaultAnimationClip;
            public bool ShouldPauseOnClipFinished() => pauseOnClipFinished;
            public float GetClipLength() => associatedClip.length;

			[Button("Refresh Cached Name", EButtonEnableMode.Always)]
			public void RefreshCachedName() => name = System.Enum.GetName(typeof(AIAnim), animationEnum);
            public void Initialise(Animator animator)
            {
                anim = animator;
				RefreshCachedName();
                SetValue(GetDefaultValue());
            }
        }
    }
	
	public enum AIAnim
	{
		Swim = 0,
		Bite,
		Feint
	}
}
