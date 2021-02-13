using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileData : ScriptableObject
    {
        public int ID;
        public string Name;
        public int BaseDamage;
        public float Movespeed;
        public float ExpireTime;
        public LayerMask TargetLayer;
        public GameObject ProjectilePrefab;

        protected virtual GameObject InstantiateProjectile(Vector3 position, Quaternion rotation, Transform parent)
        {
            return Instantiate(ProjectilePrefab, position, rotation, parent);
        }

        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}