using UnityEngine;
using Hadal.Usables.Projectiles;
//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    public class UsableHandlerInfo
    {
        public Vector3 FirePoint { get; private set; }
        public Quaternion Orientation { get; set; }
        public float ChargedTime { get; private set; }
        public Vector3 shooterVelocity { get; private set; }
        public TrapBehaviour Trap { get; set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(null, 0.0f, Vector3.zero);

        public UsableHandlerInfo() { }

        public UsableHandlerInfo(Transform fireTransform, float ChargedForce, Vector3 shooterVelocity)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedForce;
            this.shooterVelocity = shooterVelocity;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformForceInfo(Transform fireTransform, float ChargedTime, Vector3 shooterVelocity)
        {
            if (fireTransform == null) return null;
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedTime;
            this.shooterVelocity = shooterVelocity;
            return this;
        }


        #endregion
    }
}