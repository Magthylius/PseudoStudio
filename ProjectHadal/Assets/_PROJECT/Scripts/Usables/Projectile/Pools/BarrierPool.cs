using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class BarrierPool : ProjectilePool<BarrierBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Barrier Data");
            base.Start();
        }
    }
}
