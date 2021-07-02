using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    public class StaticClassManager : MonoBehaviour
    {
        public static StaticClassManager Instance;

        public delegate void StaticClassEvent();

        public event StaticClassEvent ResetEvent;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void CallForReset() => ResetEvent?.Invoke();
    }
}
