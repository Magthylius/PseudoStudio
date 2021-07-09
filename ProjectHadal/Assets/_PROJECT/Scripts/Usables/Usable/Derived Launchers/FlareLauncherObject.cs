//Created by Jet
using UnityEngine;

namespace Hadal.Usables
{
    public class FlareLauncherObject : UsableLauncherObject
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
