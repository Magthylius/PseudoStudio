using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonicGrenadePool : ProjectilePool<SonicGrenadeBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonic Grenade Data");
            base.Start();
        }
    }
}
