using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileData : ScriptableObject
    {
        public string Name;
        public int BaseDamage;
        public LayerMask TargetLayer;
        public GameObject ProjectilePrefab;
        
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}