using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Flare")]
    public class FlareLauncherData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = FlarePool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected override void DumpProjectileMethod(ProjectileObject obj)
        {
            if (obj is FlareObject flare)
            {
                FlarePool.Instance.Dump(flare);
            }
        }
    }
}