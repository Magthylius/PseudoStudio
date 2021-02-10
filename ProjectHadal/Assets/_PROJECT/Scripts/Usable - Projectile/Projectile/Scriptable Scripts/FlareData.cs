using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Projectiles/Flare Data")]
    public class FlareData : ProjectileData
    {

        public override bool DoEffect(ProjectileHandlerInfo info)
        {
            return true;
        }
    }
}