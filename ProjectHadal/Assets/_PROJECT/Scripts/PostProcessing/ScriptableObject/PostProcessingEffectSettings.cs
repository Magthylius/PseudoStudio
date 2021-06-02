using System.Collections;
using System.Collections.Generic;
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

    public struct LensDistortionSettings 
    {
        public float Intensity;
        public float XMultiplier;
        public float YMultiplier;
        public Vector2 Center;
        public float Scale;

        public LensDistortionSettings(float intensity = 0f)
        {
            Intensity = intensity;
            XMultiplier = 1f;
            YMultiplier = 1f;
            Center = new Vector2(0.5f, 0.5f);
            Scale = 1f;
        }
    }
}
