using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileData : ScriptableObject
    {
        public string Name;
        public int ProjIDInt;
        public int BaseDamage;
        public LayerMask TargetLayer;
        public GameObject ProjectilePrefab;
        public bool UseOriginal { get; set; } = true;
        
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}