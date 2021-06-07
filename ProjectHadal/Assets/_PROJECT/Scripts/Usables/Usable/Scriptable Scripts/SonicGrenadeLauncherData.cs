using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Sonic Grenade Launcher")]
    public class SonicGrenadeLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = SonicGrenadePool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.projectileID = projectileObj.Data.ProjTypeInt;
            projectileObj.projectileID += info.ProjectileID;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.Rigidbody.velocity = info.shooterVelocity;
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
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is SonicGrenadeBehaviour sonicGrenade)
            {
                if (!obj.GetComponentInParent<SonicGrenadePool>())
                {
                    sonicGrenade.Rigidbody.isKinematic = false;
                    sonicGrenade.transform.SetParent(SonicGrenadePool.Instance.transform);
                }
                SonicGrenadePool.Instance.Dump(sonicGrenade);
            }
        }
    }
}
