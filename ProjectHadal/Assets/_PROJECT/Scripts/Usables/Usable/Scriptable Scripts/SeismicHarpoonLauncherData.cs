using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Seismic Harpoon Launcher")]
    public class SeismicHarpoonLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            Debug.LogError("gay");
            var projectileObj = SeismicHarpoonPool.Instance.Scoop();
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
            if (obj is SeismicHarpoonBehaviour harpoon)
            {
                if (!obj.GetComponentInParent<SeismicHarpoonPool>())
                {
                    harpoon.Rigidbody.isKinematic = false;
                    harpoon.transform.SetParent(SeismicHarpoonPool.Instance.transform);
                }
                SeismicHarpoonPool.Instance.Dump(harpoon);
            }
        }
    }
}
