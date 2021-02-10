using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public class ProjectileObject : MonoBehaviour, IProjectile, IPoolable<ProjectileObject>
    {
        public virtual ProjectileData Data { get; set; }
        public ObjectPool<ProjectileObject> MotherPool { get; set; }

        public virtual bool Use(ProjectileHandlerInfo info)
        {
            return Data.DoEffect(info);
        }
        public void Dump()
        {
            if (MotherPool is null) return;
            Data = null;
            MotherPool.Dump(this);
        }
    }
}