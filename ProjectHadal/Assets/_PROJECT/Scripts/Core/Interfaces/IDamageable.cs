//Created by Jet
using UnityEngine;

namespace Hadal
{
    public interface IDamageable
    {
        bool TakeDamage(int damage);
        GameObject Obj { get; }
    }
}