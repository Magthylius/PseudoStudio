using UnityEngine;

namespace Hadal.AudioSystem
{
    public abstract class AudioEventData : ScriptableObject
    {
        public abstract bool Play(Vector3 position);
        public abstract void Play(AudioSource source);
        public abstract void Pause(bool isPaused);
        public abstract void Stop();
    }
}
