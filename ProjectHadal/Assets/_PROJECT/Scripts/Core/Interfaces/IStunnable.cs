using UnityEngine;

namespace Hadal
{
    public interface IStunnable
    {
        bool TryStun(float duration);
    }
}
