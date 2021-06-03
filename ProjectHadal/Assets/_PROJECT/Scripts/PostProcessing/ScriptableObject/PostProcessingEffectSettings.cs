using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

namespace Hadal.PostProcess.Settings
{
    public class PostProcessingEffectSettings
    {
        public string settingsName;
        public bool active;
    }

    [System.Serializable]
    public struct DepthOfFieldSettings
    {
        public DepthOfFieldMode Mode;
        [Min(0.1f)] public float FocusDistance;
        [Range(1f, 150f)] public float FocusLength;
        [Range(1f, 32f)] public float Aperture;
        [Range(3, 9)] public int BladeCount;
        [Range(0f, 1f)] public float BladeCurvature;
        [Range(-180f, 180f)] public float BladeRotation;

        public DepthOfFieldSettings(in DepthOfField depthOfField)
        {
            Mode = depthOfField.mode.value;
            FocusDistance = depthOfField.focusDistance.value;
            FocusLength = depthOfField.focalLength.value;
            Aperture = depthOfField.aperture.value;
            BladeCount = depthOfField.bladeCount.value;
            BladeCurvature = depthOfField.bladeCurvature.value;
            BladeRotation = depthOfField.bladeRotation.value;
        }

        public DepthOfFieldSettings(DepthOfFieldSettings depthOfField)
        {
            Mode = depthOfField.Mode;
            FocusDistance = depthOfField.FocusDistance;
            FocusLength = depthOfField.FocusLength;
            Aperture = depthOfField.Aperture;
            BladeCount = depthOfField.BladeCount;
            BladeCurvature = depthOfField.BladeCurvature;
            BladeRotation = depthOfField.BladeRotation;
        }
    }

    [System.Serializable]
    public struct LensDistortionSettings 
    {
        [Range(-1f, 1f)] public float Intensity;
        [Range(0f, 1f)] public float XMultiplier;
        [Range(0f, 1f)] public float YMultiplier;
        public Vector2 Center;
        [Range(0.1f, 5f)] public float Scale;

        public LensDistortionSettings(in LensDistortion lensDistortion)
        {
            Intensity = lensDistortion.intensity.value;
            XMultiplier = lensDistortion.xMultiplier.value;
            YMultiplier = lensDistortion.yMultiplier.value;
            Center = lensDistortion.center.value;
            Scale = lensDistortion.scale.value;
        }
        public LensDistortionSettings(LensDistortionSettings other)
        {
            Intensity = other.Intensity;
            XMultiplier = other.XMultiplier;
            YMultiplier = other.YMultiplier;
            Center = other.Center;
            Scale = other.Scale;
        }
        public LensDistortionSettings(float intensity = 0f)
        {
            Intensity = intensity;
            XMultiplier = 1f;
            YMultiplier = 1f;
            Center = new Vector2(0.5f, 0.5f);
            Scale = 1f;
        }

        public bool LerpIntensity(float targetIntensity, float speed, float tolerance = 0.01f)
        {
            Intensity = Mathf.Lerp(Intensity, targetIntensity, speed);

            if (Mathf.Abs(targetIntensity - Intensity) <= tolerance)
            {
                Intensity = targetIntensity;
                return true;
            }

            return false;
        }
        public bool LerpScale(float targetScale, float speed, float tolerance = 0.01f)
        {
            Scale = Mathf.Lerp(Scale, targetScale, speed);

            if (Mathf.Abs(targetScale - Scale) <= tolerance)
            {
                Scale = targetScale;
                return true;
            }

            return false;
        }
    }

    [System.Serializable]
    public struct ChromaticAberrationSettings
    {
        [Range(0f, 1f)] public float Intensity;

        public ChromaticAberrationSettings(in ChromaticAberration chromaticAberration)
        {
            Intensity = chromaticAberration.intensity.value;
        }
        public ChromaticAberrationSettings(ChromaticAberrationSettings other)
        {
            Intensity = other.Intensity;
        }
        public ChromaticAberrationSettings(float intensity = 0f)
        {
            Intensity = intensity;
        }

        public bool LerpIntensity(float targetIntensity, float speed, float tolerance = 0.01f)
        {
            Intensity = Mathf.Lerp(Intensity, targetIntensity, speed);

            if (Mathf.Abs(targetIntensity - Intensity) <= tolerance)
            {
                Intensity = targetIntensity;
                return true;
            }

            return false;
        }
    }
}
