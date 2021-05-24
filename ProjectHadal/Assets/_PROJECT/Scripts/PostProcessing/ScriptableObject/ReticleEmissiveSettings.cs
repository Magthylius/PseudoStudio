using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.PostProcess
{
    [CreateAssetMenu(fileName = "Reticle Emissive Settings", menuName = "PostProcessing/Reticle Emissive Settings")]
    public class ReticleEmissiveSettings : ScriptableObject
    {
        [Header("Primary Color")]
        [ColorUsage(true, true)] public Color primaryEmissiveColor;
        [Range(0f, 1f)] public float primaryEmissiveAlpha;

        [Header("Secondary Color")]
        [ColorUsage(true, true)] public Color secondaryEmissiveColor;
        [Range(0f, 1f)] public float secondaryEmissiveAlpha;

        [Header("Tertiary Color")]
        [ColorUsage(true, true)] public Color tertiaryEmissiveColor;
        [Range(0f, 1f)] public float tertiaryEmissiveAlpha;
    }
}
