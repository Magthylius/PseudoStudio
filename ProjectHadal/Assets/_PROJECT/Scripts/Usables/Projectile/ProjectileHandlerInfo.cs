using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public struct ProjectileHandlerInfo
    {
        public Transform Self { get; set; }
        public Vector3 Direction { get; private set; }
        public IDamageable IntendTarget { get; private set; }
        public static ProjectileHandlerInfo Null => new ProjectileHandlerInfo(null, Vector3.zero, null);

        public ProjectileHandlerInfo(Transform self, Vector3 direction, IDamageable target)
        {
            Self = self;
            Direction = direction;
            IntendTarget = target;
        }
    }
}