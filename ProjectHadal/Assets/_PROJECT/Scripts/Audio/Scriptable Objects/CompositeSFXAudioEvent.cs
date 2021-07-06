using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for SFX sounds that can play multiple SFX events at once. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Composite SFX")]
    public class CompositeSFXAudioEvent : AudioEventData
    {
        [SerializeField] private AudioEventData[] Composites;
        public override string Description => "Audio event meant to play other audio events for SFX sounds. Audio events put in the composite will be called all at once by the respective functions [listed below] (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 3D Composite Playing, and 2D Composite Playing functions."
                                            + "\n\nNote: Preview Button will only play 2D audio for now.";

        public override bool Play(Vector3 position)
        {
            if (Composites.IsNullOrEmpty()) return false;

            bool anySuccess = false;
            int i = -1;
            while (++i < Composites.Length)
            {
                if (Composites[i].Play(position))
                    anySuccess = true;
            }
            return anySuccess;
        }

        public override void Play(AudioSource source)
        {
            if (source == null || Composites.IsNullOrEmpty()) return;

            int i = -1;
            while (++i < Composites.Length)
                Composites[i].Play(source);
        }

        public override void Pause(bool isPaused) { }
        public override void Stop(bool isEditor = false) { }
    }
}
