using Tenshi;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal.AudioSystem
{
    [System.Serializable]
    public class AudioSourceSettings
    {
        [SerializeField, Range(0f, 1f)] private float Volume;
        [SerializeField, MinMaxSlider(.1f, 3f)] private Vector2 Pitch;
        [SerializeField, Range(0f, 1f)] private float SpatialBlend;
        [SerializeField] private AudioMixerGroup MixerGroup;

        public void AssignSettings(AudioSource source)
        {
            source.volume = Volume;
            source.pitch = Pitch.RandomBetweenXY();
            source.spatialBlend = SpatialBlend;
            source.outputAudioMixerGroup = MixerGroup;
        }
    }
}
