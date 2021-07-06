using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for Ambience sounds that can play multiple Ambience events at once. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Composite Ambience")]
    public class CompositeAmbienceAudioEvent : AudioEventData
    {
        [SerializeField] private AudioEventData[] Composites;
        public override string Description => "Audio event meant to play other audio events for Ambience. Audio events put in the composite will be called all at once by the respective functions [listed below] (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 2D Composite Playing, Composite [Un]Pausing, and Composite Stopping functions.";

        public override bool Play(Vector3 position)
        {
            return false;
        }

        public override void Play(AudioSource source)
        {
            if (source == null || Composites.IsNullOrEmpty()) return;

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
