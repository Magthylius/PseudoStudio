using UnityEngine;

namespace Tenshi.UnitySoku
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = GetComponent<T>();
        }
    }

    public abstract class SingletonSoft<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = GetComponent<T>();
        }
    }

    public class SingletonPersist<T> : Singleton<T> where T : Component
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}