using UnityEngine;

namespace Hadal.AudioSystem
{
    public abstract class AudioEventData : ScriptableObject
    {
        public abstract bool Play(Vector3 position);
        public abstract void Stop();
    }
}
