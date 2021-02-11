using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class ProjectileHandlerInfo
    {
        public IDamageable Target { get; private set; }

        public ProjectileHandlerInfo(IDamageable target)
        {
            Target = target;
        }
    }
}