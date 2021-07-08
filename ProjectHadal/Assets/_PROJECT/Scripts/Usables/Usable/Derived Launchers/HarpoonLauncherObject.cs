//Create by Jet
using UnityEngine;

namespace Hadal.Usables
{
    public class HarpoonLauncherObject : UsableLauncherObject
    {
        public override bool Use(UsableHandlerInfo info)
        {
            if (!IsActive) return false;
            info.IsPowered = IsPowered;
            base.Use(info);
            return true;
        }
    }
}
