using Tenshi.UnitySoku;
using NaughtyAttributes;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class ProjectilePool<T> : ObjectPool<T> where T : ProjectileBehaviour
    {
        [ReadOnly, SerializeField] protected ProjectileData data;

        protected override void Start()
        {
            prefab = data.ProjectilePrefab.GetComponent<T>();
            base.Start();
        }
    }
}