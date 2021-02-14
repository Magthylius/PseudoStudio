using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jey
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Torpedo")]
    public class TorpedoLauncherData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = TorpedoPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected override void DumpProjectileMethod(ProjectileObject obj)
        {
            if (obj is TorpedoObject torpedo)
            {
                TorpedoPool.Instance.Dump(torpedo);
            }
        }
    }
}