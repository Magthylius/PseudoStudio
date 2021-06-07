using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;
using Hadal.UI;

//Created by Jet, E: Jon
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Sonic Dart Launcher (Sonic Tracker)")]
    public class SonicDartLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = SonicDartPool.Instance.Scoop();
            projectileObj.projectileID = projectileObj.Data.ProjTypeInt;
            projectileObj.projectileID += info.ProjectileID;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.Rigidbody.velocity = info.shooterVelocity;
            projectileObj.WithGObjectSetActive(true);

            projectileObj.GetComponentInChildren<ImpulseMode>().OverrideForce
                (isChargable ? info.ChargedTime.Clamp01() * MaxForce : MaxForce);

            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();

            //! pass in transform data to uimanager
            if (!UIManager.IsNull) UIManager.Instance.TrackProjectile(projectileObj.transform, TrackerType.SONIC_DART);
           
            
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is SonicDartBehaviour sonicDart)
            {
                if (!obj.GetComponentInParent<SonicDartPool>())
                {
                    sonicDart.Rigidbody.isKinematic = false;
                    sonicDart.transform.SetParent(SonicDartPool.Instance.transform); ;
                }
                if (!UIManager.IsNull) UIManager.Instance.UntrackProjectile(obj.transform);
                SonicDartPool.Instance.Dump(sonicDart);
            }
        }
    }
}