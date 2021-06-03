//Created by Harry, E: Jon
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using NaughtyAttributes;
using Hadal.PostProcess.Settings;

namespace Hadal.PostProcess
{
    public class PostProcessingManager : MonoBehaviour // put this script on the post process volume
    {
        public static PostProcessingManager Instance;

        [Header("Settings")]
        [SerializeField] Volume volume;
        [SerializeField] VolumeProfile DefaultProfile;

        [Header("Underwater Image Effect")]
        public Material underwaterMat; 
        public bool underwaterEffectEnabled = true;
        [SerializeField] UnderwaterEffectData UEDataEnabled;
        [SerializeField] UnderwaterEffectData UEDataDisabled;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        void Start()
        {
            //Get profiles
            //volume = GetComponent<Volume>();
            SetUnderwaterEffectSettings(underwaterEffectEnabled);
        }

        void Update()
        {
            //! Debug use
            /*if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleUnderwaterEffect();
            }*/
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) // don't touch
        {
            Graphics.Blit(source, destination, underwaterMat);
        }

        public void ToggleEffect<T>() where T : VolumeComponent
        {
            T component;
            if (volume.profile.TryGet(out component))
                component.active = !component.active;
        }

        #region Underwater Effect
        [Button("Toggle Underwater Effect")]
        public void ToggleUnderwaterEffect()
        {
            underwaterEffectEnabled = !underwaterEffectEnabled;
            SetUnderwaterEffectSettings(underwaterEffectEnabled);
        }

        public void SetUnderwaterEffectSettings(bool enable)
        {
            underwaterEffectEnabled = enable;

            if (underwaterEffectEnabled) SetUnderwaterEffectSettings(UEDataEnabled);
            else SetUnderwaterEffectSettings(UEDataDisabled);
        }

        public void SetUnderwaterEffectSettings(UnderwaterEffectData data)
        {
            SetUnderwaterEffectSettings(data.effectMaterial, data.noiseFrequency, data.noiseSpeed, data.noiseScale, data.pixelOffset, data.depthStart, data.depthDistance);
        }

        public void SetUnderwaterEffectSettings(Material mat, float _noiseFrequency, float _noiseSpeed, float _noiseScale, float _pixelOffset, float _depthStart, float _depthDistance)
        {
            mat.SetFloat("_NoiseFrequency", _noiseFrequency);
            mat.SetFloat("_NoiseSpeed", _noiseSpeed);
            mat.SetFloat("_NoiseScale", _noiseScale);
            mat.SetFloat("_PixelOffset", _pixelOffset);
            mat.SetFloat("_DepthStart", _depthStart);
            mat.SetFloat("_DepthDistance", _depthDistance);
        } 
        #endregion

        public void ToggleFog() 
        {
            RenderSettings.fog = !RenderSettings.fog;
        }

        public void EditDepthOfField(float targetFocusDistance, float targetFocalLength, float _focusSpeed)
        {
            DepthOfField dof;
            if (volume.profile.TryGet(out dof))
            {
                dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, targetFocusDistance, Time.deltaTime * _focusSpeed);
                dof.focalLength.value = Mathf.Lerp(dof.focalLength.value, targetFocalLength, Time.deltaTime * _focusSpeed);
                return;
            }

            Debug.LogError("Tried to edit depth of field, but not found!");
        }

        public void EditLensDistortion(LensDistortionSettings settings)
        {
            LensDistortion ld;
            if (CurrentVolumeTryGet(out ld))
            {
                ld.intensity.Override(settings.Intensity);
                ld.xMultiplier.Override(settings.XMultiplier);
                ld.yMultiplier.Override(settings.YMultiplier);
                ld.center.Override(settings.Center);
                ld.scale.Override(settings.Scale);

                return;
            }

            Debug.LogError("Tried to edit lens distortion, but not found!");
        }

        public void EditChromaticAberration(ChromaticAberrationSettings settings)
        {
            ChromaticAberration ca;
            if (CurrentVolumeTryGet(out ca))
            {
                ca.intensity.Override(settings.Intensity);

                return;
            }

            Debug.LogError("Tried to edit chromatic aberration, but not found!");
        }

        public bool CurrentVolumeTryGet<T>(out T component) where T : VolumeComponent
        {
            bool r = volume.profile.TryGet(out component);
            return r;
        }

        public bool DefaultVolumeTryGet<T>(out T component) where T : VolumeComponent
        {
            bool r = DefaultProfile.TryGet(out component);
            return r;
        }
    }
}
