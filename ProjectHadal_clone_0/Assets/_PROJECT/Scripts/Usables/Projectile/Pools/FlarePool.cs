using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class FlarePool : ProjectilePool
    {
        protected override void Awake()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Flare Data");
            base.Awake();
        }
    }
}