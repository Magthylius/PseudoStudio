using UnityEngine;
using Hadal.Usables.Projectiles;
//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    public class UsableHandlerInfo
    {
        public Vector3 FirePoint { get; private set; }
        public Quaternion Orientation { get; private set; }
        public float ChargedTime { get; private set; }
        public TrapBehaviour Trap { get; set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(null, 0.0f);

        public UsableHandlerInfo() { }

        public UsableHandlerInfo(Transform fireTransform, float ChargedForce)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedForce;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformForceInfo(Transform fireTransform, float ChargedTime)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedTime;
            return this;
        }


        #endregion
    }
}