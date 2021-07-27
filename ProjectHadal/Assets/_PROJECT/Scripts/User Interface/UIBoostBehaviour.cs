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
            //LeftGauge.fillAmount = Mathf.Lerp(LeftGauge.fillAmount, targetProgress, lerpSpeed * Time.deltaTime);
            //RightGauge.fillAmount = Mathf.Lerp(RightGauge.fillAmount, targetProgress, lerpSpeed * Time.deltaTime);

            //LeftGauge.fillAmount = 
            if (Lerp.Float(LeftGauge.fillAmount, targetProgress, lerpSpeed)) LeftGauge.fillAmount = targetProgress;
            RightGauge.fillAmount = LeftGauge.fillAmount;
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

            if (targetProgress < segmentDivident)
            {
                LeftGauge.color = GaugeEmptyColor;
                RightGauge.color = GaugeEmptyColor;
            }
            else if (targetProgress >= 1f)
            {
                LeftGauge.color = GaugeFullColor;
                RightGauge.color = GaugeFullColor;
            }
            else
            {
                LeftGauge.color = GaugePartialColor;
                RightGauge.color = GaugePartialColor;
            }
        }
    }
}
