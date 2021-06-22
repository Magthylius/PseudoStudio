//Created by Jet
using UnityEngine;

namespace Hadal
{
    public interface ISlowable
    {
        void UpdateSlowStacks(int change);
        void ResetAllSlowStacks();
    }
}