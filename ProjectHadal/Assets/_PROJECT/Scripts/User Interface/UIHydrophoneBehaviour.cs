using System;
using System.Collections;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Hadal.UI
{
    public class UIHydrophoneBehaviour : MonoBehaviour
    {
        [SerializeField] private RectTransform volumeBarParent;
        [SerializeField] private float intensityPerUnit = 0.01f;
        [SerializeField] private float minimumDistance = 500f;
        
        [Space(10f)]
        [SerializeField] private float maxIntensity = 200f;
        [SerializeField] private bool allowRandomIntensity = true;
        [SerializeField] private float randomIntensityRange = 20f;
        [SerializeField] private float randomIntensityChangeTime = 0.5f;

        private float randomIntensity = 0f;

        [Space(10f)] 
        [SerializeField, NaughtyAttributes.ReadOnly] private float currentIntensity = 0f;
        [SerializeField, NaughtyAttributes.ReadOnly] private int barsNeeded;
        [SerializeField, NaughtyAttributes.ReadOnly] private List<Image> volumeBars;
        [SerializeField, NaughtyAttributes.ReadOnly] private Transform aiTransform;
        [SerializeField, NaughtyAttributes.ReadOnly] private float aiDistance = 0f;

        private float barsPerIntensity;
        private Transform playerTransform;
        
        private bool isAIInitialized = false;
        private bool isPlayerInitialized = false;
        
        void Start()
        {
            volumeBars = new List<Image>(volumeBarParent.GetComponentsInChildren<Image>());
            barsPerIntensity = maxIntensity / volumeBars.Count;

            foreach (var child in volumeBars)
            {
                child.enabled = false;
            }

            StartCoroutine(GenerateRandomIntensity());

            IEnumerator GenerateRandomIntensity()
            {
                while (allowRandomIntensity)
                {
                    randomIntensity = Random.Range(-randomIntensityRange, randomIntensityRange);
                    yield return new WaitForSeconds(randomIntensityChangeTime);
                }
            }
        }

        void FixedUpdate()
        {
            if (!isAIInitialized || !isPlayerInitialized) return;

            aiDistance = minimumDistance - (aiTransform.position - playerTransform.position).magnitude;
            currentIntensity = Mathf.Clamp((aiDistance + randomIntensity) * intensityPerUnit, 0f, maxIntensity);
            barsNeeded = Mathf.FloorToInt((currentIntensity ) * barsPerIntensity);
            barsNeeded = Mathf.Clamp(barsNeeded, 0, volumeBars.Count);

            for (int i = 0; i < volumeBars.Count; i++)
            {
                volumeBars[i].enabled = i <= barsNeeded;
            }
        }

        public void InjectAIDependencies(Transform AItransform)
        {
            aiTransform = AItransform;
            isAIInitialized = true;
        }
        
        public void InjectPlayerDependencies(Transform pTransform)
        {
            playerTransform = pTransform;
            isPlayerInitialized = true;
        }

        public void Activate() => gameObject.SetActive(true);
        public void Deactivate() => gameObject.SetActive(false);
    }

}