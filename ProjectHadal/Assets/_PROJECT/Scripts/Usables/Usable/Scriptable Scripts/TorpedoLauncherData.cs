using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jey, edited by Jon
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Torpedo")]
    public class TorpedoLauncherData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = TorpedoPool.Instance.Scoop();
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);

            if (projectileObj.PPhysics != null) projectileObj.PPhysics.LaunchProjectile();
            //else projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
            else Debug.LogWarning("PPhysics didnt init");
        }

        protected override void DumpProjectileMethod(ProjectileBehaviour obj)
        {
            if (obj is TorpedoBehaviour torpedo)
            {
                TorpedoPool.Instance.Dump(torpedo);
            }
        }
    }
}