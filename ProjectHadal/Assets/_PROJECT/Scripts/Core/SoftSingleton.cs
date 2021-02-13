using UnityEngine;

namespace Hadal
{
    public abstract class SoftSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = GetComponent<T>();
        }
    }
}