using Hadal.Usables.Projectiles;
using UnityEngine;
using Tenshi;

//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Flare Launcher")]
    public class FlareLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = FlarePool.Instance.Scoop();
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
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is FlareBehaviour flare)
            {
                flare.Rigidbody.isKinematic = false;
                flare.transform.SetParent(FlarePool.Instance.transform); 
                FlarePool.Instance.Dump(flare);
            }
        }
    }
}