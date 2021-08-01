using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal
{
    public class SettingsManager : MonoBehaviour
    {
        public AudioMixer SFXMixer;
        public AudioMixer AMBMixer;
        
        public SettingsSliderHandler MasterSlider;
        public SettingsSliderHandler SFXSlider;
        public SettingsSliderHandler AMBSlider;

        private static string PrefMasterVol = "MasterVol";
        private static string PrefSFXVol = "SFXVol";
        private static string PrefAMBVol = "AMBVol";

        private bool initialized = false;
        
        public void Start()
        {
            //! Load Settings
            if (PlayerPrefs.HasKey(PrefMasterVol))
            { 
                MasterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefMasterVol));
            }

            if (PlayerPrefs.HasKey(PrefSFXVol))
            {
                SFXSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefSFXVol));
            }

            if (PlayerPrefs.HasKey(PrefAMBVol))
            {
                AMBSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefAMBVol));
            }

            UpdateAudioMixer();
            
        }

        public void UpdateAudioMixer()
        {
            float sfxConverted = Mathf.Log10(SFXSlider.Value * MasterSlider.Value) * 20f;
            float ambConverted = Mathf.Log10(AMBSlider.Value * MasterSlider.Value) * 20f;

            if (SFXMixer.GetFloat("MasterVol", out float sfxV))
            {
                SFXMixer.SetFloat("MasterVol", sfxConverted);
            }
            
            if (AMBMixer.GetFloat("MasterVol", out float ambV))
            {
                AMBMixer.SetFloat("MasterVol", ambConverted);
            }

            //Debug.LogWarning($"Updated {PlayerPrefs.GetFloat(PrefMasterVol)}");
            PlayerPrefs.SetFloat(PrefMasterVol, MasterSlider.Value);
            PlayerPrefs.SetFloat(PrefSFXVol, SFXSlider.Value);
            PlayerPrefs.SetFloat(PrefAMBVol, AMBSlider.Value);
        }
    }
}
