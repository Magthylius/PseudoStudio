using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonicDartPool : ProjectilePool<SonicDartBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonic Dart Data");
            base.Start();
        }
    }
}