using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Magthylius;
using Magthylius.DataFunctions;

namespace Hadal.UI
{
    public class UIPowerHandler : MonoBehaviour
    {
        [SerializeField] private Image mainPowerImage;
        [SerializeField] private Image powerGauge;
        [SerializeField] private float powerChargeTime;
        [SerializeField] private Color powerActivatedColor;
        [SerializeField] private Color powerDeactivatedColor;
        [SerializeField] private ParticleSystem powerReadyVFX;
        
        private Timer powerTimer;

        private void Start()
        {
            powerTimer = new Timer(powerChargeTime, false, false, true);
            powerTimer.TargetTickedEvent.AddListener(ActivatePower);
        }

        private void LateUpdate()
        {
            powerTimer.Tick(Time.fixedDeltaTime);
            powerGauge.fillAmount = powerTimer.Progress;
        }

        void ActivatePower()
        {
            powerTimer.Pause();

            mainPowerImage.color = powerActivatedColor;
            powerReadyVFX.Emit(1);
        }

        void DeactivatePower()
        {
            powerTimer.Reset();
            powerTimer.Continue();

            mainPowerImage.color = powerDeactivatedColor;
        }
    }
}
