using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using NaughtyAttributes;
using System.Linq;
using ReadOnly = Tenshi.ReadOnlyAttribute;
using Tenshi.UnitySoku;
using Hadal.Networking;
using ExitGames.Client.Photon;

namespace Hadal.AI
{
    public class AIAnimationManager : MonoBehaviour
    {
		[SerializeField] private Animator animator;
		[SerializeField] private float defaultAnimLerpTime;
		[SerializeField] private float stunnedAnimSpeedMultiplier = 0.2f;
		[SerializeField] private float deathAnimStopTime = 1f;
		[SerializeField] private List<AnimationFloat> floatData;
		
		private bool onMasterClient;
		private bool isSpeedStopped;
		private bool shouldUseStunMultiplier;
		private AIBrain _brain;
		private Coroutine mainLerpRoutine;
		private Coroutine independentLerpRoutine;
		private Coroutine stopDelaySubroutine;
		private const string SpeedMultiplierString = "SpeedMultiplier";
		
		public void Initialise(AIBrain brain, bool isMasterClient)
		{
			//! Class initialisations
			_brain = brain;
			onMasterClient = isMasterClient;
			if (animator == null) animator = GetComponent<Animator>();
			floatData.ForEach(f => f.Initialise(animator));
			SetSpeed(1f);
			
			//! Events
			if (onMasterClient)
			{
				_brain.OnStunnedEvent += HandleStunEvent;
			}
			else
			{
				NetworkEventManager.Instance.AddListener(ByteEvents.AI_PLAY_ANIMATION, Receive_SetAnimation);
				NetworkEventManager.Instance.AddListener(ByteEvents.AI_SET_ANIMATION_SPEED, Receive_UpdateAnimationSpeed);
			}

			//! Default animation state
			SetAnimation(AIAnim.Swim);
		}

		private void Send_SetAnimation(AIAnim animType, float customLerpTime)
		{
			if (NetworkEventManager.Instance == null || !onMasterClient) //! only master client can send this
                return;
            
            object[] content = new object[] { (int)animType, customLerpTime };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_PLAY_ANIMATION, content, SendOptions.SendReliable);
		}
		
		private void Receive_SetAnimation(EventData eventData)
		{
			object[] content = (object[])eventData.CustomData;
			AIAnim animType = (AIAnim)(int)content[0];
			float customLerpTime = (float)content[1];
			
			SetAnimation(animType, customLerpTime);
		}

		private void Send_UpdateAnimationSpeed()
		{
			if (NetworkEventManager.Instance == null || !onMasterClient) //! only master client can send this
                return;
			
			object[] content = new object[] { GetSpeed() };
			NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_SET_ANIMATION_SPEED, content, SendOptions.SendReliable);
		}

		private void Receive_UpdateAnimationSpeed(EventData eventData)
		{
			object[] content = (object[])eventData.CustomData;
			float speed = (float)content[0];
			SetSpeed(speed);
		}

		private void OnDestroy()
		{
			if (_brain != null)
				_brain.OnStunnedEvent -= HandleStunEvent;
		}
		
		public void SetAnimation(AIAnim animType, float customAnimLerpTime = -1f)
		{
			StopAllRunningCoroutines();
			AnimationFloat curFloat = GetAnimationFloatFromAnimType(animType);
			if (!curFloat.ShouldBeChangedIndependently())
			{
				if (animType == AIAnim.Death)
					StartCoroutine(LerpSpeedToDeathStop());
				
				mainLerpRoutine = StartCoroutine(LerpAnimation(animType, customAnimLerpTime));
			}
			else
			{
				if (independentLerpRoutine != null) return;
				independentLerpRoutine = StartCoroutine(LerpIndependentAnimation(animType, customAnimLerpTime));
			}

			Send_SetAnimation(animType, customAnimLerpTime);
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
			}
			if (currentFloat.ShouldRefreshTree())
				RefreshBlendTree();

			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpTime;
				currentFloat.LerpToFocusedValue(percent);
				otherFloats.ForEach(f => f.LerpToUnfocusedValue(percent));
				yield return null;
			}
			
			mainLerpRoutine = null;
		}

		private IEnumerator LerpIndependentAnimation(AIAnim animType, float customAnimLerpTime)
		{
			AnimationFloat currentFloat = GetAnimationFloatFromAnimType(animType);

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
			}
			if (currentFloat.ShouldRefreshTree())
				RefreshBlendTree();

			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpTime;
				currentFloat.LerpToFocusedValue(percent);
				yield return null;
			}

			percent = 0f;
			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpTime;
				currentFloat.LerpToUnfocusedValue(percent);
				yield return null;
			}

			independentLerpRoutine = null;
		}

		private IEnumerator LerpSpeedToDeathStop()
		{
			float percent = 0f;
			float lerpSpeed = 1f / deathAnimStopTime;
			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpSpeed;
				float speed = Mathf.Lerp(GetSpeed(), 0f, percent);
				
				SetSpeed(speed);
				Send_UpdateAnimationSpeed();
				yield return null;
			}
			SetSpeed(0f);
			Send_UpdateAnimationSpeed();
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

		internal void StopSpeed()
		{
			if (!onMasterClient)
				return;
			
			animator.SetFloat(SpeedMultiplierString, 0f);
			Send_UpdateAnimationSpeed();
		}
        internal void ResetSpeed()
		{
			if (!onMasterClient)
				return;
			
			animator.SetFloat(SpeedMultiplierString, 1f * GetStunnedMultiplier());
			Send_UpdateAnimationSpeed();
		}
		private void UpdateStunnedSpeed()
		{
			if (!onMasterClient)
				return;
			
			animator.SetFloat(SpeedMultiplierString, GetSpeed() * GetStunnedMultiplier());
			Send_UpdateAnimationSpeed();
		}
		private float GetSpeed() => animator.GetFloat(SpeedMultiplierString);
		internal void SetSpeed(float value) => animator.SetFloat(SpeedMultiplierString, value);
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
			[SerializeField] private bool isIndependentChange;
            [SerializeField] private bool pauseOnClipFinished;
			[SerializeField] private bool refreshTreeOnClipStart = true;
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
			public bool ShouldBeChangedIndependently() => isIndependentChange;
            public bool ShouldPauseOnClipFinished() => pauseOnClipFinished;
			public bool ShouldRefreshTree() => refreshTreeOnClipStart;
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
		Aggro,
		Hurt,
		Death
	}
}
