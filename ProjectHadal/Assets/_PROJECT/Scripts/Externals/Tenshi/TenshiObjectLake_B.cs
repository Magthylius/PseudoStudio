using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

//! C: Jet, E: Jon
namespace Tenshi.UnitySoku
{
    public interface IPoolable<T> where T : Component
    {
        event Action<T> DumpEvent;
        void Dump();
    }

    public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : Component
    {
        [SerializeField] protected T prefab;
        [SerializeField] protected int initialCount;
        [SerializeField] protected bool instantiateWithCoroutine;
        protected Queue<T> pool = new Queue<T>();
        public Action<Queue<T>> InitialisationCompleted;


        bool poolingFinished = false;

        protected virtual void Start()
        {
            if (instantiateWithCoroutine)
            {
                StartCoroutine(StartRoutine());
                return;
            }
            Add(initialCount.Clamp0());
            InitialisationCompleted?.Invoke(pool);
        }

        private IEnumerator StartRoutine()
        {
            int i = -1;
            while (++i < initialCount.Clamp0())
            {
                Dump(Instantiate(prefab, transform));
                yield return null;
            }

            poolingFinished = true;
            InitialisationCompleted?.Invoke(pool);
            yield return null;
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
            int i = -1;
            while(++i < count)
                Dump(Instantiate(prefab, transform));
        }

        public static void DumpAll<TPoolable>() where TPoolable : Component
        {
            var allPoolables = FindObjectsOfType<GameObject>()
                                .Select(p => p.GetComponent<IPoolable<TPoolable>>())
                                .Where(p => p != null)
                                .ToArray();
            
            int i = -1;
            while(++i < allPoolables.Length)
                allPoolables[i].Dump();
        }

        public bool PoolingFinished => poolingFinished;
    }
}