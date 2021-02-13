using System;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    public interface IPoolable<T> where T : Component
    {
        Action<T> DumpEvent { get; }
        void Dump();
    }
}