using System;
using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIBoostBehaviour : MonoBehaviour
    {
        enum BoostStage
        {
            Full,
            Partial,
            Empty
        }
        
        public Image LeftGauge;
        public Image RightGauge;
        public List<Image> BoostIcons;
        [Space(10f)]
        public float lerpSpeed = 5f;
        public Color GaugeFullColor;
        public Color GaugePartialColor;
        public Color GaugeEmptyColor;

        private bool isInitialized = false;
        private float dodgeCount;
        private float segmentDivident;
        private float targetProgress;

        private BoostStage currentStage = BoostStage.Full;
        private Action<BoostStage> boostStageChangedEvent;

        private void OnEnable()
        {
            boostStageChangedEvent += OnBoostStageChanged;
        }

        private void OnDisable()
        {
            boostStageChangedEvent -= OnBoostStageChanged;
        }

        private void LateUpdate()
        {
            if (!isInitialized) return;
            
            LeftGauge.fillAmount = Lerp.Snap(LeftGauge.fillAmount, targetProgress, lerpSpeed * Time.deltaTime);
            RightGauge.fillAmount = LeftGauge.fillAmount;
        }

        public void Initialize(int dashCount)
        {
            isInitialized = true;
            segmentDivident = 1.0f / (float) dashCount;
            dodgeCount = dashCount;
            currentStage = BoostStage.Full;
            OnBoostStageChanged(currentStage);
        }
        
        public void UpdateGaugeValue(float value)
        {
            if (!isInitialized) return;
            
            float progress = value * segmentDivident;
            progress = Mathf.Clamp(progress, 0f, dodgeCount);
            
            targetProgress = progress;
            //RightGauge.fillAmount = progress;
            BoostStage updatedStage;

            if (targetProgress < segmentDivident) updatedStage = BoostStage.Empty;
            else if (targetProgress >= 1f) updatedStage = BoostStage.Full;
            else updatedStage = BoostStage.Partial;

            if (updatedStage != currentStage)
            {
                boostStageChangedEvent.Invoke(updatedStage);
            }
        }

        void OnBoostStageChanged(BoostStage stage)
        {
            if (!isInitialized) return;
            
            currentStage = stage;

            Color currentColor = Color.white;
            
            switch (stage)
            {
                case BoostStage.Empty:
                    currentColor = GaugeEmptyColor;
                    break;
                case BoostStage.Partial:
                    currentColor = GaugePartialColor;
                    break;
                case BoostStage.Full:
                    currentColor = GaugeFullColor;
                    break;
            }
            
            LeftGauge.color = currentColor;
            RightGauge.color = currentColor;
            BoostIcons.ForEach(o => o.color = currentColor);
        }
    }
}
