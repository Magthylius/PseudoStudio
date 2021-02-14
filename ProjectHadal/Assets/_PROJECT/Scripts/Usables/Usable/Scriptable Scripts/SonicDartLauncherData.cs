using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Sonic Dart")]
    public class SonicDartLauncherData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = SonicDartPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected override void DumpProjectileMethod(ProjectileObject obj)
        {
            if (obj is SonicDartObject sonicDart)
            {
                SonicDartPool.Instance.Dump(sonicDart);
            }
        }
    }
}