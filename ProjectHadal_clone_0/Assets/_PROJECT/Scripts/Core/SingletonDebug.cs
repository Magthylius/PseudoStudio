using UnityEngine;

namespace Hadal
{
    public abstract class SingletonDebug<T> : MonoBehaviourDebug where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = GetComponent<T>();
        }
    }
}