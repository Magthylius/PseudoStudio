using System.Collections;
using UnityEngine;

namespace Tenshi.UnitySoku
{
    public class CoroutineData
    {
        public Coroutine Coroutine { get; private set; }
        public object Result { get; private set; }
        private IEnumerator _target;

        public CoroutineData(MonoBehaviour owner, IEnumerator target)
        {
            _target = target;
            Coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (_target.MoveNext())
            {
                Result = _target.Current;
                yield return Result;
            }
        }
    }
}