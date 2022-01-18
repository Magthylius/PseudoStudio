using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Harpoon Launcher")]
    public class HarpoonLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = HarpoonPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.projectileID = info.ProjectileID;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.Rigidbody.velocity = info.shooterVelocity;
            projectileObj.SetAimedPoint(info.AimedPoint);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.SubscribeModeEvent();

            ImpulseMode impluseMode = projectileObj.GetComponentInChildren<ImpulseMode>();
            impluseMode.OverrideForce
                (isChargable ? info.ChargedTime.Clamp01() * MaxForce : MaxForce, info.IsPowered);

            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is HarpoonBehaviour harpoon)
            {
                if (!obj.GetComponentInParent<HarpoonPool>())
                {
                    harpoon.Rigidbody.isKinematic = false;
                    harpoon.transform.SetParent(HarpoonPool.Instance.transform);
                }
                HarpoonPool.Instance.Dump(harpoon);
            }
        }
    }
}
