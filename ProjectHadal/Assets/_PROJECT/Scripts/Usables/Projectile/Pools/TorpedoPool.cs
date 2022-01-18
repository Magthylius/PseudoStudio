using UnityEngine;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoPool : ProjectilePool<TorpedoBehaviour>
    {
        protected override void Awake()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Torpedo Data");
            base.Awake();
        }
    }
}