using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class TrapPool : ProjectilePool<TrapBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Trap Data");
            base.Start();
        }
    }
}
