using UnityEngine;
using Hadal.Usables.Projectiles;
//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    public class UsableHandlerInfo
    {
        public int ProjectileID { get; private set; }
        public Vector3 FirePoint { get; private set; }
        public Quaternion Orientation { get; set; }
        public bool IsPowered { get; set; }
        public float ChargedTime { get; private set; }
        public Vector3 shooterVelocity { get; private set; }
        public Vector3 AimedPoint { get; set; }
        public TrapBehaviour Trap { get; set; }
        public bool LocallyFired { get; private set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(0,null, 0.0f, Vector3.zero);

        public UsableHandlerInfo() { }

        public UsableHandlerInfo(int projectileID, Transform fireTransform, float ChargedForce, Vector3 shooterVelocity)
        {
            ProjectileID = projectileID;
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedForce;
            this.shooterVelocity = shooterVelocity;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformForceInfo(int projectileID,Transform fireTransform, float ChargedTime, Vector3 shooterVelocity, Vector3 aimedPoint, bool isLocal)
        {
            if (fireTransform == null) return null;
            ProjectileID = projectileID;
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedTime = ChargedTime;
            this.shooterVelocity = shooterVelocity;
            AimedPoint = aimedPoint;
            LocallyFired = isLocal;
            return this;
        }


        #endregion
    }
}