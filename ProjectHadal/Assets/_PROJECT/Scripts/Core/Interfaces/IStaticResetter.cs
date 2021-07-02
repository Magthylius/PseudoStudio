using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    public interface IStaticResetter
    {
        /// <summary>
        /// Used for StaticClassManager to call reset. Remember to tie the reset to StaticClassManager's ResetEvent.
        /// </summary>
        void Reset();
    }
}
