//Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class UnderwaterEffect : MonoBehaviour
{
    public Material _mat; // set this material to UnderwaterImageEffect material

    [Range(0.001f, 0.1f)]
    public float _pixelOffset;
    [Range(0.1f, 20f)]
    public float _noiseScale;
    [Range(0.1f, 20f)]
    public float _noiseFrequency;
    [Range(0.1f, 30f)]
    public float _noiseSpeed;


    void Start()
    {
        
    }

    void Update()
    {
        _mat.SetFloat("_NoiseFrequency", _noiseFrequency);
        _mat.SetFloat("_NoiseSpeed", _noiseSpeed);
        _mat.SetFloat("_NoiseScale", _noiseScale);
        _mat.SetFloat("_PixelOffset", _pixelOffset);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, _mat);
    }
}
