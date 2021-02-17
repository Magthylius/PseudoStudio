using UnityEngine;

namespace Hadal.Usables.Projectiles
{
    [CreateAssetMenu(menuName = "Projectiles/Empty Data")]
    public class EmptyProjectileData : ProjectileData
    {
        protected override GameObject InstantiateProjectile(Vector3 position, Quaternion rotation, Transform parent)
        {
            Debug.LogWarning("Instantiated an empty projectile object. Is there a missing reference?");
            return new GameObject("EmptyProjectile GameObject");
        }

        public static ProjectileData Get()
            => (ProjectileData)Resources.Load(PathManager.EmptyProjectileDataPath);
    }
}