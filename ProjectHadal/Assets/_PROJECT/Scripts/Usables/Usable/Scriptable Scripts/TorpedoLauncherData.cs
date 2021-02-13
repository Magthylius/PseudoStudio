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
            var projectileObj = TorpedoPool.Instance.Scoop().WithGObjectSetActive(true);
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent = CallOnDump;
            projectileObj.gameObject.transform.position = info.FirePoint;
            projectileObj.gameObject.transform.rotation = info.Orientation;
            projectileObj.Rigidbody.AddForce(info.Direction * info.Force);
        }

        private void CallOnDump(ProjectileObject obj)
        {
            if (obj is TorpedoObject torpedo)
            {
                TorpedoPool.Instance.Dump(torpedo);
            }
        }
    }
}