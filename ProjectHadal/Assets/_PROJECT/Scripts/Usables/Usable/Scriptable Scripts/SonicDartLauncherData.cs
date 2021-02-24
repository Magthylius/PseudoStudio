using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;
//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Sonic Dart Launcher (Sonic Tracker)")]
    public class SonicDartLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = SonicDartPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);

            projectileObj.GetComponentInChildren<ImpulseMode>().OverrideForce
                (isChargable ? info.ChargedTime.Clamp01() * MaxForce : MaxForce);
            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();
            //projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is SonicDartBehaviour sonicDart)
            {
                SonicDartPool.Instance.Dump(sonicDart);
            }
        }
    }
}