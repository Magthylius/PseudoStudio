//Create by Jet
using UnityEngine;

namespace Hadal.Usables
{
    public class HarpoonLauncherObject : UsableLauncherObject
    {
        [SerializeField] int poweredUpReserveCapacity;
        public override void PowerUp()
        {
            maxReserveCapacity = poweredUpReserveCapacity;
            SetDefaults();
            return;
        }
        /* public override bool Use(UsableHandlerInfo info)
         {
             if (!IsActive) return false;
             info.IsPowered = IsPowered;
             base.Use(info);
             return true;
         }*/
    }
}
