//Created by Jet, edited by Jin
using UnityEngine;
using Hadal.Usables.Projectiles;
using System;

namespace Hadal.Usables
{
    public class TrapLauncherObject : UsableLauncherObject
    {
        [SerializeField] ProjectileBehaviour activeTrap;
        public override event Action<UsableLauncherObject> OnFire;
        
        public override bool Use(UsableHandlerInfo info)
        {
            if (!IsActive) return false;
    
            if (activeTrap && activeTrap.gameObject.activeSelf)
            {
                bool triggered = activeTrap.TriggerBehavior();
                if (triggered)
                {
                    activeTrap = null;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            
            info.Trap = TrapPool.Instance.Scoop();
            activeTrap = info.Trap;

            base.Use(info);
            print("work with me");
            return false;
        }
    }
}
