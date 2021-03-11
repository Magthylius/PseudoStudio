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

        private void Start()
        {
            Data.projectileScooped += updateActiveTrap;
        }

        private void updateActiveTrap(ProjectileBehaviour trapBehaviour)
        {
            activeTrap = trapBehaviour;
        }

        public override bool Use(UsableHandlerInfo info)
        {
            if(activeTrap)
            {
                bool triggered = activeTrap.TriggerBehavior();
                if(triggered) activeTrap = null;
                return true;
            }

            if (!IsActive) return false;
            OnFire?.Invoke(this);
            LaunchToDestination(info);
            return true;
        }
    }
}
