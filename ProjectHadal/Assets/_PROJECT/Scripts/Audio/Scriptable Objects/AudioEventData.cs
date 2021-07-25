using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Base class for all audio event that are played from derived scriptable objects. </summary>
    public abstract class AudioEventData : ScriptableObject
    {
        public virtual string Description { get; } = "An audio event.";
        public abstract bool Play(Transform followPosTransform);
        public abstract bool Play(Vector3 position);
        public abstract void Play(AudioSource source);
        public virtual void Play(int track) { }
        public virtual void PlayOneShot(Vector3 position) {}
        public virtual void Play2D() { }
        public abstract void Pause(bool isPaused);
        public abstract void Stop(bool isEditor = false);
    }
}
