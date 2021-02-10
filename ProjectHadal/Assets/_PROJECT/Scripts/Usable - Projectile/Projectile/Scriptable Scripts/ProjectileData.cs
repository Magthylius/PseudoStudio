using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public abstract class ProjectileData : ScriptableObject
    {
        public int ID;
        public string Name;
        public int BaseDamage;
        public GameObject ProjectilePrefab;
        protected GameObject projectileObject;

        public abstract bool DoEffect(ProjectileHandlerInfo info);
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