using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIBoostBehaviour : MonoBehaviour
    {
        public Image LeftGauge;
        public Image RightGauge;
        public float lerpSpeed = 5f;
        public Color GaugeFullColor;
        public Color GaugePartialColor;
        public Color GaugeEmptyColor;

        private float dodgeCount;
        private float segmentDivident;
        private float targetProgress;

        private void LateUpdate()
        {
            LeftGauge.fillAmount = Mathf.Lerp(LeftGauge.fillAmount, targetProgress, lerpSpeed * Time.deltaTime);
            RightGauge.fillAmount = Mathf.Lerp(RightGauge.fillAmount, targetProgress, lerpSpeed * Time.deltaTime);
        }

        public void Initialize(int dashCount)
        {
            segmentDivident = 1.0f / (float) dashCount;
            dodgeCount = dashCount;
            //Debug.LogWarning(dodgeCount);
        }
        
        public void UpdateGaugeValue(float value)
        {
            //Debug.LogWarning(value);
            float progress = value * segmentDivident;
            progress = Mathf.Clamp(progress, 0f, dodgeCount);
            
            targetProgress = progress;
            //RightGauge.fillAmount = progress;
        }
    }
}
