using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Hadal.PostProcess
{
    public class PostProcessingEffectSettings : ScriptableObject
    {
        public string settingsName;
        public bool active;
    }

    [CreateAssetMenu(fileName = "DepthOfField Setting", menuName = "PostProcessing/DeptOfField")]
    public class DepthOfFieldSettings : PostProcessingEffectSettings
    {
        public DepthOfFieldMode mode;
        public float focalDistance;
        [Range(0f, 350f)] public float focalLength;
        [Range(20f, 32f)] public float aperture;
        [Range(3, 9)] public int bladeCount;
        [Range(0f, 1f)] public float bladeCurvature;
        [Range(-180f, 180f)] public float bladeRotation;

        public void Override(in DepthOfField dof)
        {
            dof.mode = new DepthOfFieldModeParameter(mode, true);
            dof.focusDistance = new MinFloatParameter(focalDistance, 0f, true);
        }
    }
}
