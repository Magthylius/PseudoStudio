using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

namespace Hadal.AI
{
    public class AIAnimationManager : MonoBehaviour
    {
		[SerializeField] private Animator animator;
		[SerializeField] private float defaultAnimLerpTime;
		[SerializeField] private List<AnimationData> data;
		[SerializeField] private string animXString;
		[SerializeField] private string animYString;
		
		private bool _onMasterClient;
		private AIBrain _brain;
		private Coroutine lerpRoutine;
		
		public void Initialise(AIBrain brain, bool onMasterClient)
		{
			_brain = brain;
			_onMasterClient = onMasterClient;
			if (animator == null) animator = GetComponent<Animator>();
			
			//! Default animation state
			SetAnimation(AIAnim.Swim);
		}
		
		public void SetAnimation(AIAnim animType, float customAnimLerpTime = -1f)
		{
			StopAllRunningCoroutines();
			lerpRoutine = StartCoroutine(LerpAnimation(animType, customAnimLerpTime));
			_brain.Send_SetAnimation(animType, customAnimLerpTime);
		}
		
		private IEnumerator LerpAnimation(AIAnim animType, float customAnimLerpTime)
		{
			Vector2 currentVec = new Vector2(animator.GetFloat(animXString), animator.GetFloat(animYString));
			Vector2 target = GetVectorFromAnimType(animType);
			float lerpTime = customAnimLerpTime > 0f ? customAnimLerpTime : defaultAnimLerpTime;
			float percent = 0f;
			
			while (percent < 1f)
			{
				percent += Time.deltaTime * lerpTime;
				currentVec = Vector2.Lerp(currentVec, target, percent);
				UpdateAnimationValues(currentVec.x, currentVec.y);
				yield return null;
			}
			
			lerpRoutine = null;
		}
		
		private void UpdateAnimationValues(float x, float y)
		{
			animator.SetFloat(animXString, x);
			animator.SetFloat(animYString, y);
		}
		
		private Vector2 GetVectorFromAnimType(AIAnim animType)
		{
			int i = -1;
			while (++i < data.Count)
			{
				if (data[i].associatedAnim == animType)
					return data[i].animTreeOffset;
			}
			
			return Vector2.zero;
		}
		
		private void StopAllRunningCoroutines()
		{
			if (lerpRoutine != null)
				StopCoroutine(lerpRoutine);
			lerpRoutine = null;
		}
		
		[System.Serializable]
        private class AnimationData
        {
            public AIAnim associatedAnim;
            public Vector2 animTreeOffset;
        }
    }
	
	public enum AIAnim
	{
		Swim = 0,
		Bite,
		Feint
	}
}
