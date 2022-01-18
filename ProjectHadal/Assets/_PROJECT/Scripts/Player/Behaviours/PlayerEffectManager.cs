using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Player.Behaviours;
using Hadal.PostProcess;
using Hadal.PostProcess.Settings;
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

        public PlayerController Controller;
        public float effectSpeed = 1f;
        
        [Foldout("Camera")] public bool AllowCameraEffects = true;
        [Foldout("Camera")] public Camera playerCamera;
        [Foldout("Camera")] public float cameraMaxEffectFOV;
        private float cameraOriginalFOV;
        
        [Foldout("Chromatic Aberration")] public bool AllowChromaticAberrationEffect = true;
        [Foldout("Chromatic Aberration")] public float caMaxIntensity = 1f;
        private float caOriginalIntensity;
        private ChromaticAberration caComp;

        [Header("External Effects")] 
        public bool AllowMotionBlur;
        public float MotionBlurMaxIntensity;
        private float targetMotionBlurIntensity;
        private MotionBlurSettings mbSettings = new MotionBlurSettings(0f);
        
        private VolumeProfile volumeProfile;
        private bool allowEffects;

        [Header("Pause effects")] 
        public float PauseEffectSpeed = 5f;
        public bool AllowDepthOfField = true;
        public float ClosedBlurFDistance = 3.0f;
        public float OpenedBlurFDistance  = 0.1f;

        private bool allowPauseEffects = false;
        private DepthOfField dof;
        private float targetBlurFDistance;
        
        public void Start()
        {
            volumeProfile = UIVolume.profile;
            
            if (AllowChromaticAberrationEffect)
            {
                if (volumeProfile.TryGet(out caComp))
                    caOriginalIntensity = caComp.intensity.value;
                else AllowChromaticAberrationEffect = false;
            }

            if (AllowDepthOfField)
            {
                if (volumeProfile.TryGet(out dof))
                { dof.focusDistance.Override(ClosedBlurFDistance); }
                else AllowDepthOfField = false;
            }

            cameraOriginalFOV = playerCamera.fieldOfView;

            if (AllowDepthOfField)
            {
                Controller.UI.PauseMenuOpened += ActivatePauseBlur;
                Controller.UI.PauseMenuClosed += DeactivatePauseBlur;
            }
        }

        private void LateUpdate()
        {
            if (allowEffects)
            {
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

            if (allowPauseEffects)
            {
                float dofVal = dof.focusDistance.value;
                if (Tolerance(dofVal, targetBlurFDistance, 0.01f))
                {
                    dof.focusDistance.Override(targetBlurFDistance);
                    allowPauseEffects = false;
                }
                else 
                    dof.focusDistance.Override(Mathf.Lerp(dofVal, targetBlurFDistance, PauseEffectSpeed * Time.deltaTime));
            }
        }

        private void OnDestroy()
        {
            if (AllowDepthOfField)
            {
                Controller.UI.PauseMenuOpened -= ActivatePauseBlur;
                Controller.UI.PauseMenuClosed -= DeactivatePauseBlur;
            }
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

        public void ActivatePauseBlur()
        {
            //dof.focusDistance.Override(OpenedBlurFDistance);
            targetBlurFDistance = OpenedBlurFDistance;
            allowPauseEffects = true;
        }
        
        public void DeactivatePauseBlur()
        {
            //dof.focusDistance.Override(ClosedBlurFDistance);
            targetBlurFDistance = ClosedBlurFDistance;
            allowPauseEffects = true;
        }

        public void SetTargetMotionBlurIntensity(float normalizedIntensity)
        {
            targetMotionBlurIntensity = MotionBlurMaxIntensity * normalizedIntensity;
        }
    }
}
