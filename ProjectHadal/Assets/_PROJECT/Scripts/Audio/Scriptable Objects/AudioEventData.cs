using UnityEngine;

namespace Hadal.AudioSystem
{
    public abstract class AudioEventData : ScriptableObject
    {
        public abstract void Play(Vector3 position);
        public abstract void Stop();
    }
}
