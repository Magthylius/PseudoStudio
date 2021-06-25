// Created by Harry
using System.Collections;
using UnityEngine;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class FogScene : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]private Color targetColor;
        [SerializeField]private float targetFogEndDistance;

        [SerializeField][Range(0f, 1f)] float lerpTime;

        private void OnTriggerEnter(Collider other)
        {

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
    }
}