// Created by Jet; Editted by Jin
using UnityEngine;

namespace Hadal.Usables
{
    public class LureLauncherObject : UsableLauncherObject
    {
        [SerializeField] private bool isActive;
        public bool LureIsActive => isActive;
        [SerializeField] private float lureCD;
        [SerializeField] private float lureCDMax;

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            if(lureCD > 0)
                lureCD -= fixedDeltaTime;
        }

        public override bool Use(UsableHandlerInfo info)
        {
            if (isActive) // stop luring and start cooldown
            {
                isActive = false;
                lureCD = lureCDMax;
                isEquipLocked = false;
            }
            else if (!isActive && lureCD <= 0)  // not using lure and its off cd
            {
                isActive = true;
                isEquipLocked = true;
            }

            return true;
        }
    }
}
