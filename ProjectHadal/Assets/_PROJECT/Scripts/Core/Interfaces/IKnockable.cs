//Created by Jet
using UnityEngine;

namespace Hadal
{
    public interface IKnockable
    {
        bool TryToKnock(Vector3 force, float duration);
    }
}