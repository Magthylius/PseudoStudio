using NaughtyAttributes;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class ProjectilePool : ObjectPool<ProjectileObject>
    {
        [ReadOnly, SerializeField] protected ProjectileData data;

        protected override void Awake()
        {
            prefab = data.ProjectilePrefab.GetComponent<ProjectileObject>();
            base.Awake();
        }
    }
}