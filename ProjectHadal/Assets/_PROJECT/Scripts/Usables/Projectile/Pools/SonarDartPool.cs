using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SonarDartPool : ProjectilePool<SonarDartObject>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonar Dart Data");
            base.Start();
        }
    }
}