using System;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    public interface IPoolable<T> where T : Component
    {
        event Action<T> DumpEvent;
        void Dump();
    }
}