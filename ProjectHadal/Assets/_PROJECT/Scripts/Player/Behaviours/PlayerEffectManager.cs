using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Player.Behaviours;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Magthylius.Utilities.MathUtil;
using NaughtyAttributes;

namespace Hadal.Player
{
    public class PlayerEffectManager : MonoBehaviour
    {
        public Volume UIVolume;

        public float effectSpeed = 1f;
        
        [Foldout("Camera")] public bool AllowCameraEffects = true;
        [Foldout("Camera")] public Camera playerCamera;
        [Foldout("Camera")] public float cameraMaxEffectFOV;
        private float cameraOriginalFOV;
        
        [Foldout("Chromatic Aberration")] public bool AllowChromaticAberrationEffect = true;
        [Foldout("Chromatic Aberration")] public float caMaxIntensity = 1f;
        private float caOriginalIntensity;

        private ChromaticAberration caComp;

        private VolumeProfile volumeProfile;

        private bool allowEffects;
        
        public void Start()
        {
            volumeProfile = UIVolume.profile;
            
            if (AllowChromaticAberrationEffect)
            {
                if (volumeProfile.TryGet(out caComp))
                    caOriginalIntensity = caComp.intensity.value;
                else AllowChromaticAberrationEffect = false;
            }

            cameraOriginalFOV = playerCamera.fieldOfView;
        }

        private void LateUpdate()
        {
            if (!allowEffects) return;

            bool caReady = false;
            bool camReady = false;

            if (Tolerance(caComp.intensity.value, caOriginalIntensity, 0.01f))
            {
                caComp.intensity.Override(caOriginalIntensity);
                caReady = true;
            }
            else caComp.intensity.Override(Mathf.Lerp(caComp.intensity.value, caOriginalIntensity, effectSpeed * Time.deltaTime));

            if (Tolerance(playerCamera.fieldOfView, cameraOriginalFOV, 0.01f))
            {
                playerCamera.fieldOfView = cameraOriginalFOV;
                camReady = true;
            }
            else playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, cameraOriginalFOV, effectSpeed * Time.deltaTime);

            allowEffects = !(camReady && caReady);
        }

        public void HandleDamageEffect(float normalizedIntensity)
        {
            if (AllowChromaticAberrationEffect)
            {
                caComp.intensity.Override(caMaxIntensity * normalizedIntensity);
            }

            if (AllowCameraEffects)
            {
                playerCamera.fieldOfView = Mathf.Lerp(cameraMaxEffectFOV, cameraOriginalFOV, normalizedIntensity);
            }
            
            allowEffects = true;
        }
        
    }
}
