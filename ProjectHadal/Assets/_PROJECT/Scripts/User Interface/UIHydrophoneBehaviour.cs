using System;
using System.Collections;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIHydrophoneBehaviour : MonoBehaviour
    {
        [SerializeField] private RectTransform volumeBarParent;
        [SerializeField] private float intensityPerUnit = 0.01f;
        [SerializeField] private float minimumDistance = 500f;
        [SerializeField] private float maxIntensity = 200f;

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
        }

        void FixedUpdate()
        {
            if (!isAIInitialized || !isPlayerInitialized) return;

            aiDistance = minimumDistance - (aiTransform.position - playerTransform.position).magnitude;
            currentIntensity = Mathf.Clamp(aiDistance * intensityPerUnit, 0f, maxIntensity);
            barsNeeded = Mathf.FloorToInt(currentIntensity * barsPerIntensity);
            barsNeeded = Mathf.Clamp(barsNeeded, 0, volumeBars.Count);

            for (int i = 0; i < volumeBars.Count; i++)
            {
                volumeBars[i].enabled = i <= barsNeeded;
            }
        }

        public void InjectAIDependencies(Transform AItransform)
        {
            //Debug.LogWarning("Hydrophone init!");
            
            aiTransform = AItransform;
            isAIInitialized = true;
        }
        
        public void InjectPlayerDependencies(Transform pTransform)
        {
            //Debug.LogWarning("Hydrophone init!");
            
            playerTransform = pTransform;
            isPlayerInitialized = true;
        }
    }

}