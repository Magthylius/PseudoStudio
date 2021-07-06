using Tenshi;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal.AudioSystem
{
    [CreateAssetMenu(menuName = "Audio Event/Standard")]
    public class StandardAudioEvent : AudioEventData
    {
        [SerializeField] private AudioClip[] Clips;
        [SerializeField, Range(0f, 1f)] private float Volume;
        [SerializeField, Range(0.1f, 3f)] private float Pitch;
        [SerializeField, Range(0f, 1f)] private float SpatialBlend;
        [SerializeField] private AudioMixerGroup MixerGroup;

        public override void Play(Vector3 position)
        {
            if (Clips.IsNullOrEmpty()) return;
            
            // AudioSource source;
            // source.
        }

        public override void Stop() { }
    }
}
