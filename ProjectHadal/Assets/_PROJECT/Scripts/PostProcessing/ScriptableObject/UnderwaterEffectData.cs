using UnityEngine;

[CreateAssetMenu(fileName = "Underwater Effect Data", menuName = "PostProcessing/Underwater Effect")]
public class UnderwaterEffectData : ScriptableObject
{
    public Material effectMaterial;
    [Range(0.000f, 0.1f)] public float pixelOffset;
    [Range(0.0f, 20f)] public float noiseScale;
    [Range(0.0f, 20f)] public float noiseFrequency;
    [Range(0.0f, 30f)] public float noiseSpeed;
    public float depthStart = 0;
    public float depthDistance = 100;
}
