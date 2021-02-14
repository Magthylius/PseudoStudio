using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonarDartPool : ProjectilePool<SonarDartBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonar Dart Data");
            base.Start();
        }
    }
}