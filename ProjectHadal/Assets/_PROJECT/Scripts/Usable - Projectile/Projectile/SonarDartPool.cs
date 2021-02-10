using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public class SonarDartPool : ProjectilePool
    {
        protected override void Awake()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Sonar Dart Data");
            base.Awake();
        }
    }
}