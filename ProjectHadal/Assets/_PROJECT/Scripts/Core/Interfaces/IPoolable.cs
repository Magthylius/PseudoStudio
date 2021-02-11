using System.Collections.Generic;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    public interface IPoolable<T> where T : Component
    {
        ObjectPool<T> MotherPool { get; }
        void Dump();
    }
}