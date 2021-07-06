using Tenshi;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal.AudioSystem
{
    [System.Serializable]
    public class AudioSourceSettings
    {
        [Header("General")]
        [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 Volume = Vector2.one;
        [SerializeField, MinMaxSlider(-3f, 3f)] private Vector2 Pitch = Vector2.one;
        [SerializeField, Range(0f, 1f)] private float SpatialBlend = 0.7f;
        [SerializeField, Range(0, 256)] private int Priority = 128;
        [SerializeField, Range(-1f, 1f)] private float StereoPan = 0;
        [SerializeField, Range(0f, 1.1f)] private float ReverbZoneMix = 1f;
        
        [Header("Effects & Lifecycle")]
        [SerializeField] private AudioMixerGroup OutputMixerGroup;
        [SerializeField] private bool Mute = false;
        [SerializeField] private bool BypassEffects = false;
        [SerializeField] private bool BypassListenerEffects = false;
        [SerializeField] private bool BypassReverbZones = false;
        [SerializeField] private bool PlayOnAwake = false;
        [SerializeField] private bool Loop = false;

        [Header("3D Sound")]
        [SerializeField, Range(0f, 5f)] private float DopplerLevel;
        [SerializeField, Range(0f, 360f)] private float Spread;
        [SerializeField] private AudioRolloffMode VolumeRolloff = AudioRolloffMode.Logarithmic;
        [SerializeField] private float MinDistance = 1f;
        [SerializeField] private float MaxDistance = 500f;

        public void AssignSettings(ref AudioSource source)
        {
            source.volume = Volume.RandomBetweenXY();
            source.pitch = Pitch.RandomBetweenXY();
            source.spatialBlend = SpatialBlend;
            source.priority = Priority;
            source.panStereo = StereoPan;
            source.reverbZoneMix = ReverbZoneMix;

            source.outputAudioMixerGroup = OutputMixerGroup;
            source.mute = Mute;
            source.bypassEffects = BypassEffects;
            source.bypassListenerEffects = BypassListenerEffects;
            source.bypassReverbZones = BypassReverbZones;
            source.playOnAwake = PlayOnAwake;
            source.loop = Loop;

            source.dopplerLevel = DopplerLevel;
            source.spread = Spread;
            source.rolloffMode = VolumeRolloff;
            source.minDistance = MinDistance;
            source.maxDistance = MaxDistance;
        }
    }
}
