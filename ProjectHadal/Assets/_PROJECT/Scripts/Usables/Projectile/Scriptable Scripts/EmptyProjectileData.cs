using UnityEngine;

namespace Hadal.Usables.Projectiles
{
    [CreateAssetMenu(menuName = "Projectiles/Empty Data")]
    public class EmptyProjectileData : ProjectileData
    {
        public static ProjectileData Get()
            => (ProjectileData)Resources.Load(PathManager.EmptyProjectileDataPath);
    }
}