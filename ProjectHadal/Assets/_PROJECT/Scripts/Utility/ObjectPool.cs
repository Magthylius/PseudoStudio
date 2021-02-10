using System.Collections.Generic;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    public class ObjectPool<T> : MonoBehaviour where T : Component
    {
        public static ObjectPool<T> Instance { get; private set; }
        [SerializeField] private bool isDoNotDestroy = false;
        [SerializeField] protected T prefab;
        private Queue<T> pool = new Queue<T>();

        protected virtual void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            HandleSettings();
        }

        public T Scoop()
        {
            if(pool.IsEmpty()) Add(1);
            return pool.Dequeue();
        }

        public void Dump(T returnObj)
        {
            returnObj.gameObject.SetActive(false);
            pool.Enqueue(returnObj);
        }

        private void Add(int count)
        {
            var obj = Instantiate(prefab);
            Dump(obj);
        }

        private void HandleSettings()
        {
            if(isDoNotDestroy) DontDestroyOnLoad(gameObject);
        }
    }
}