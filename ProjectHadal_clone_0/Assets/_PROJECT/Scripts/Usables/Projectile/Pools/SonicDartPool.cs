using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonicDartPool : ProjectilePool
    {
        protected override void Awake()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonic Dart Data");
            base.Awake();
        }
    }
}