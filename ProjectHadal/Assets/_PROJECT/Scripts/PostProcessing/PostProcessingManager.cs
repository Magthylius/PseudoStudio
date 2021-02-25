//Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Hadal.PostProcess
{
    public class PostProcessingManager : MonoBehaviour // put this script on the post process volume
    {
        // Post Process Volume
        private Volume volume;
        private Tonemapping tone;
        private Bloom bloom;
        private Vignette vignette;
        private ChromaticAberration chroma;
        private DepthOfField depth;
        private FilmGrain grain;
        // Underwater Image Effect
        public Material underwaterMat; // assign mat "UnderwaterImageEffect"
        [Range(0.001f, 0.1f)]
        public float pixelOffset;
        [Range(0.1f, 20f)]
        public float noiseScale;
        [Range(0.1f, 20f)]
        public float noiseFrequency;
        [Range(0.1f, 30f)]
        public float noiseSpeed;

        public float depthStart = 0;
        public float depthDistance = 100;

        public bool UnderwaterEffect; // toggle this

        void Start()
        {
            //Get profiles
            volume = GetComponent<Volume>();
            volume.profile.TryGet(out tone);
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chroma);
            volume.profile.TryGet(out depth);
            volume.profile.TryGet(out grain);

            if(UnderwaterEffect)
                SetUnderwaterEffectSettings(noiseFrequency, noiseSpeed, noiseScale, pixelOffset, depthStart, depthDistance);
            else
                SetUnderwaterEffectSettings(0, 0, 0, 0, 0, 1000);
        }

        //private void Update() //uncomment this if you wanna change the values real time
        //{
        //    SetUnderwaterEffectSettings(noiseFrequency, noiseSpeed, noiseScale, pixelOffset, depthStart, depthDistance);
        //}

        public void SetUnderwaterEffectSettings(float _noiseFrequency, float _noiseSpeed, float _noiseScale, float _pixelOffset, float _depthStart, float _depthDistance) // set settings
        {
            underwaterMat.SetFloat("_NoiseFrequency", _noiseFrequency);
            underwaterMat.SetFloat("_NoiseSpeed", _noiseSpeed);
            underwaterMat.SetFloat("_NoiseScale", _noiseScale);
            underwaterMat.SetFloat("_PixelOffset", _pixelOffset);
            underwaterMat.SetFloat("_DepthStart", _depthStart);
            underwaterMat.SetFloat("_DepthDistance", _depthDistance);
        }

        public void DisableUnderwaterEffect() // disable the effect
        {
            UnderwaterEffect = false;
            SetUnderwaterEffectSettings(0, 0, 0, 0, 0, 1000);
        }

        public void ToggleToneMapping() // toggles tone
        {
            if (tone.active)
                tone.active = false;
            else
                tone.active = true;
        }
        public void ToggleBloom() // toggles bloom
        {
            if (bloom.active)
                bloom.active = false;
            else
                bloom.active = true;
        }
        public void ToggleVignette() // toggles... you get it
        {
            if (vignette.active)
                vignette.active = false;
            else
                vignette.active = true;
        }
        public void ToggleChromaticAberration()
        {
            if (chroma.active)
                chroma.active = false;
            else
                chroma.active = true;
        }
        public void ToggleDepthOfField()
        {
            if (depth.active)
                depth.active = false;
            else
                depth.active = true;
        }
        public void ToggleFilmGrain()
        {
            if (grain.active)
                grain.active = false;
            else
                grain.active = true;
        }

        public void ToggleFog() // toggles fog duhhhh
        {
            if (RenderSettings.fog == true)
                RenderSettings.fog = false;
            else
                RenderSettings.fog = true;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) // don't touch
        {
            Graphics.Blit(source, destination, underwaterMat);
        }
    }
}
