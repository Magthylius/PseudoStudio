using System.Collections;
using UnityEngine;

namespace Tenshi.UnitySoku
{
    public class CoroutineData
    {
        public Coroutine Coroutine { get; private set; }
        
        /// <summary> The result obtained after yielding the Coroutine in this data for at least 1 frame. </summary>
        public object Result { get; private set; }
        private MonoBehaviour _owner;

        public CoroutineData(MonoBehaviour owner, IEnumerator target)
        {
            if (owner == null) return;
            _owner = owner;
            Coroutine = _owner.StartCoroutine(Run());

            IEnumerator Run()
            {
                while (target.MoveNext())
                {
                    Result = target.Current;
                    yield return Result;
                }
            }
        }

        public void Stop()
        {
            if (Coroutine == null) return; 
            _owner.StopCoroutine(Coroutine);
        }

        ~CoroutineData()
        {
            if (Coroutine == null) return;
            if (_owner != null) _owner.StopCoroutine(Coroutine);
            Coroutine = null;
            _owner = null;
        }
    }
}