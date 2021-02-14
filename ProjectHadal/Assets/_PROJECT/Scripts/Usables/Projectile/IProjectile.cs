using System;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public interface IProjectile
    {
        ProjectileData Data { get; }
        Rigidbody Rigidbody { get; }
        event Action<bool> OnHit;
    }
}