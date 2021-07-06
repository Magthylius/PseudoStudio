using Tenshi;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal.AudioSystem
{
    [System.Serializable]
    public class AudioSourceSettings
    {
        [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 Volume = Vector2.one;
        [SerializeField, MinMaxSlider(.1f, 3f)] private Vector2 Pitch = Vector2.one;
        [SerializeField, Range(0f, 1f)] private float SpatialBlend = 0.7f;
        [SerializeField] private AudioMixerGroup MixerGroup;

        public void AssignSettings(ref AudioSource source)
        {
            source.volume = Volume.RandomBetweenXY();
            source.pitch = Pitch.RandomBetweenXY();
            source.spatialBlend = SpatialBlend;
            source.outputAudioMixerGroup = MixerGroup;
        }
    }
}
