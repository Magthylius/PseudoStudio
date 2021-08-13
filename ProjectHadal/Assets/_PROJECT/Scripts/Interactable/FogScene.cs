// Created by Harry
using System.Collections;
using UnityEngine;
using System.Linq;
using Tenshi;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class FogScene : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]private Color targetColor;
        [SerializeField][Range(0f, 0.05f)]private float targetFogDensity;

        [SerializeField][Range(0f, 0.01f)] float lerpTime;

        [SerializeField] private LayerMask reactiveMask;

        float percent = 0f;

        private void OnTriggerEnter(Collider other)
        {
            if(CanCollide(other))
            {
                //Debug.Log("Fogger");
                //StopAllCoroutines();
                StartCoroutine(colorLerpIn());
            }
        }

        IEnumerator colorLerpIn()
        {
            percent = 0f;
			float speed = 1f / lerpTime;
            while (percent < 1f)
            {
                percent += Time.deltaTime * speed;
                UpdateValues();
                yield return null;
            }
            percent = 1f;
			UpdateValues();
			
			void UpdateValues()
			{
				RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, percent);
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, percent);
			}
        }

        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMask);
    }
}