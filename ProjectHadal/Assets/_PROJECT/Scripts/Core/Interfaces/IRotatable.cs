using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    public interface IRotatable
    {
        void AddRotation(Vector3 normalizedDirection, float force);
    }
}
