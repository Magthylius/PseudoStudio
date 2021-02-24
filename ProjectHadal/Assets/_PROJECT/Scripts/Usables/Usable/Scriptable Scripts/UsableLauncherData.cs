using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    public abstract class UsableLauncherData : ScriptableObject
    {
        public string Name;
        public ProjectileData ProjectileData;
        public bool isChargable;
        public float MaxChargeTimer;

        public void ToggleProjectile(bool shouldToggle)
        {
            if (!shouldToggle) return;
            ProjectileData.UseOriginal = !ProjectileData.UseOriginal;
        }

        /// <summary> This method should do an effect when <see cref="UsableLauncherObject.Use"/> is called. Can be overriden by
        /// subclasses for custom behaviour. </summary>
        public virtual void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = FlarePool.Instance.Scoop().WithGObjectSetActive(true);
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.GetComponentInChildren<ImpulseMode>().force 
            projectileObj.WithGObjectSetActive(true);
            projectileObj.PPhysics.LaunchProjectile();
        }

        /// <summary> Dump method that returns spawned projectiles (if any) to its respective pool. Must be overriden by subclasses
        /// to implement behaviour. </summary>
        protected virtual void DumpProjectileMethod(ProjectileBehaviour obj) { }
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}
