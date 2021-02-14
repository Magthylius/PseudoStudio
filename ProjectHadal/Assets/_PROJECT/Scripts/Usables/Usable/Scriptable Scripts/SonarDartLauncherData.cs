using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Sonar Dart")]
    public class SonarDartLauncherData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = SonarDartPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected override void DumpProjectileMethod(ProjectileObject obj)
        {
            if (obj is SonarDartObject sonarDart)
            {
                SonarDartPool.Instance.Dump(sonarDart);
            }
        }
    }
}