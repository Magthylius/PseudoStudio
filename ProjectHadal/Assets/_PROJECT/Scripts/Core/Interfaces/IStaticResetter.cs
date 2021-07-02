using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    public interface IStaticResetter
    {
        
        /*
            private void OnEnable()
            {
                StaticClassManager.Instance.ResetEvent += Reset;
            }
            
            private void OnDisable()
            {
                StaticClassManager.Instance.ResetEvent -= Reset;
            }
        */
        
        /// <summary>
        /// Used for StaticClassManager to call reset. Remember to tie the reset to StaticClassManager's ResetEvent.
        /// </summary>
        void Reset();
    }
}
