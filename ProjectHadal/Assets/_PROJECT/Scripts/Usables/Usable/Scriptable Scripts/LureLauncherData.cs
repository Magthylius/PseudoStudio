using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;
using Hadal.UI;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Lure Launcher")]
    public class LureLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = LurePool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.Rigidbody.velocity = info.shooterVelocity;
            projectileObj.WithGObjectSetActive(true);

            projectileObj.GetComponentInChildren<ImpulseMode>().OverrideForce
                (isChargable ? info.ChargedTime.Clamp01() * MaxForce : MaxForce);

            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is LureBehaviour sonicDart)
            {
                if (!obj.GetComponentInParent<LurePool>())
                {
                    sonicDart.Rigidbody.isKinematic = false;
                    sonicDart.transform.SetParent(LurePool.Instance.transform); ;
                }
                UIManager.Instance.UntrackProjectile(obj.transform);
                LurePool.Instance.Dump(sonicDart);
            }
        }
    }
}
