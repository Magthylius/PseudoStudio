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
        [SerializeField]private float targetFogEndDistance;

        [SerializeField][Range(0f, 0.01f)] float lerpTime;

        [SerializeField] private LayerMask reactiveMask;

        private void OnTriggerEnter(Collider other)
        {
            if(CanCollide(other))
                StartCoroutine(colorLerpIn());
        }

        IEnumerator colorLerpIn()
        {
            for(int i = 1; i < 1000; i++)
            {
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, lerpTime);
                RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, targetFogEndDistance, lerpTime);
                yield return null;
            }
        }

        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMask);
    }
}