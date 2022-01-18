using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class LurePool : ProjectilePool<LureBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Lure Data");
            base.Start();
        }
    }
}
