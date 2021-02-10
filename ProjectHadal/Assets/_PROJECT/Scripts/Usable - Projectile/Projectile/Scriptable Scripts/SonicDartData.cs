using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Projectiles/Sonic Dart Data")]
    public class SonicDartData : ProjectileData
    {
        
        public override bool DoEffect(ProjectileHandlerInfo info)
        {
            return true;
        }
    }
}