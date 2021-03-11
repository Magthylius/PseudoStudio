using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Trap Launcher")]
    public class TrapLauncherData : UsableLauncherData
    {
        public override event projectileScoopEvent projectileScooped;
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = TrapPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.SubscribeModeEvent();

            ImpulseMode impluseMode = projectileObj.GetComponentInChildren<ImpulseMode>();

            if (isChargable)
            {
                bool isModeSwap = info.ChargedTime.Clamp01() > ModeToggleTreshold;
                impluseMode.OverrideForce(info.ChargedTime.Clamp01() * MaxForce, isModeSwap);
            }
            else
            {
                impluseMode.OverrideForce(MaxForce);
            }

            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();

            projectileScooped.Invoke(projectileObj);
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is TrapBehaviour sonicGrenade)
            {
                if (!obj.GetComponentInParent<TrapPool>())
                {
                    sonicGrenade.Rigidbody.isKinematic = false;
                    sonicGrenade.transform.SetParent(TrapPool.Instance.transform);
                }
                TrapPool.Instance.Dump(sonicGrenade);
            }
        }
    }
}
