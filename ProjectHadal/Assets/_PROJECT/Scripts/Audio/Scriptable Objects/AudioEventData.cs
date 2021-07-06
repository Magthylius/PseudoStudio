using UnityEngine;

namespace Hadal.AudioSystem
{
    public abstract class AudioEventData : ScriptableObject
    {
        public abstract bool Play(Vector3 position, AudioSource source = null);
        public abstract void Stop();
    }
}
