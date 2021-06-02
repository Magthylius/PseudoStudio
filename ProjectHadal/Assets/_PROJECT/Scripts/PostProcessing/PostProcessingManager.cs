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

        Volume volume;

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
            volume = GetComponent<Volume>();
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
            if (volume.profile.TryGet(out ld))
            {
                ld.intensity = new ClampedFloatParameter(settings.Intensity, ld.intensity.min, ld.intensity.max, ld.intensity.overrideState);
                ld.xMultiplier = new ClampedFloatParameter(settings.XMultiplier, ld.xMultiplier.min, ld.xMultiplier.max, ld.xMultiplier.overrideState);
                ld.yMultiplier = new ClampedFloatParameter(settings.YMultiplier, ld.yMultiplier.min, ld.yMultiplier.max, ld.yMultiplier.overrideState);
                ld.center = new Vector2Parameter(settings.Center, ld.center.overrideState);
                ld.scale = new ClampedFloatParameter(settings.Scale, ld.scale.min, ld.scale.max, ld.scale.overrideState);

                return;
            }

            Debug.LogError("Tried to edit lens distortion, but not found!");
        }
    }
}
