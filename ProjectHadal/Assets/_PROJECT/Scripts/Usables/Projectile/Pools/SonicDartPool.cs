using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonicDartPool : ProjectilePool<SonicDartObject>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonic Dart Data");
            base.Start();
        }
    }
}