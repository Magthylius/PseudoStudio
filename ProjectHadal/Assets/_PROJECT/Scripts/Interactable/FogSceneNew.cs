// Created by Harry
using System.Collections;
using UnityEngine;
using System.Linq;
using Tenshi;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class FogSceneNew : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]private Color targetColor;
        [SerializeField][Range(0f, 0.05f)]private float targetFogDensity;

        [SerializeField]float lerpTime;

        [SerializeField]private LayerMask reactiveMask;

        float percent = 0f;

        private void OnTriggerStay(Collider other)
        {
            if(CanCollide(other))
            {
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, lerpTime);
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, lerpTime);
            }
        }
        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMask);
    }
}