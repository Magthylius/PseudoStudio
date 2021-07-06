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

        public override bool Play(Vector3 position)
        {
            if (Clips.IsNullOrEmpty()) return false;
            
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                manager.PlaySFXAt(position);
                return true;
            }
            return false;
        }

        public override void Stop() { }
    }
}
