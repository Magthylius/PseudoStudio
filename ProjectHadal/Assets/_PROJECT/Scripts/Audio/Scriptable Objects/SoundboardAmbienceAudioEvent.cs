using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for Ambience sounds that can play multiple Ambience events at once. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Soundboard Ambience")]
    public class SoundboardAmbienceAudioEvent : AudioEventData
    {
        [SerializeField] private AudioEventData[] Composites;
        public override string Description => "Audio event meant to play other audio events for Ambience. Audio events put in the composite will be called all at once by the respective functions [listed below] (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 2D Composite Playing, Soundboard [Un]Pausing, and Soundboard Stopping functions.";

        public override bool Play(Transform followPosTransform) => false;
        public override bool Play(Vector3 position) => false;

        public override void Play(AudioSource source)
        {
            if (Composites.IsNullOrEmpty()) return;

            int i = -1;
            while (++i < Composites.Length)
                Composites[i].Play(source);
        }

        public override void Pause(bool isPaused)
        {
            if (Composites.IsNullOrEmpty()) return;

            int i = -1;
            while (++i < Composites.Length)
                Composites[i].Pause(isPaused);
        }
        public override void Stop(bool isEditor = false)
        {
            if (Composites.IsNullOrEmpty()) return;

            int i = -1;
            while (++i < Composites.Length)
                Composites[i].Stop(isEditor);
        }
    }
}
