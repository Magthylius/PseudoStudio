using Tenshi;
using UnityEngine;
using UnityEngine.Audio;

namespace Hadal.AudioSystem
{
    [CreateAssetMenu(menuName = "Audio Event/Standard")]
    public class StandardAudioEvent : AudioEventData
    {
        [SerializeField] private AudioClip[] Clips;
        [SerializeField] private AudioSourceSettings Settings;

        public override bool Play(Vector3 position, AudioSource source = null)
        {
            if (Clips.IsNullOrEmpty()) return false;
            if (Application.isPlaying)
            {
                var manager = AudioManager.Instance;
                if (manager != null)
                {
                    manager.PlayAudioAt(position);
                    return true;
                }
            }
            else
            {
                if (source != null)
                {
                    Settings.AssignSettings(ref source);
                    source.clip = Clips.RandomElement();
                    source.Play();
                }
            }

            return false;
        }

        public override void Stop() { }
    }
}
