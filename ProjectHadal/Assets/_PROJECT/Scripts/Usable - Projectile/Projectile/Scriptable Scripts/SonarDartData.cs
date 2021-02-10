using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Projectiles/Sonar Dart Data")]
    public class SonarDartData : ProjectileData
    {
        
        public override bool DoEffect(ProjectileHandlerInfo info)
        {
            return true;
        }
    }
}