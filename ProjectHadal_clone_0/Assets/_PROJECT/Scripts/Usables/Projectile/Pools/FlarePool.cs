using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class FlarePool : ProjectilePool<FlareBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Flare Data");
            base.Start();
        }
    }
}