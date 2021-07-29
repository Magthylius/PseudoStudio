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
        
        [Foldout("Chromatic Aberration")] public bool AllowChromaticAberrationEffect;
        [Foldout("Chromatic Aberration")] public float caMaxIntensity = 1f;
        [Foldout("Chromatic Aberration")] public float caLerpSpeed = 1f;
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
        }

        private void LateUpdate()
        {
            if (!allowEffects) return;
            
            caComp.intensity.Override(Mathf.Lerp(caComp.intensity.value, caOriginalIntensity, caLerpSpeed * Time.deltaTime));
            if (Tolerance(caComp.intensity.value, caOriginalIntensity, 0.01f))
            {
                caComp.intensity.Override(caOriginalIntensity);

                allowEffects = false;
            }
        }

        public void HandleDamageEffect(float intensity)
        {
            if (AllowChromaticAberrationEffect)
            {
                caComp.intensity.Override(caMaxIntensity * intensity);
                allowEffects = true;
            }
        }
        
    }
}
