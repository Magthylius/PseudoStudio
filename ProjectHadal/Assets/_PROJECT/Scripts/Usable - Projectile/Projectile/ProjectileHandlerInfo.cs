using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
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