using Hadal.Usables.Projectiles;
using UnityEngine;

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
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is HarpoonBehaviour harpoon)
            {
                HarpoonPool.Instance.Dump(harpoon);
            }
        }
    }
}
