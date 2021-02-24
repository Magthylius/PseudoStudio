using UnityEngine;

//Created by Jet, editted by Jin
namespace Hadal.Usables
{
    public struct UsableHandlerInfo
    {
        public Vector3 FirePoint { get; private set; }
        public Quaternion Orientation { get; private set; }
        public float ChargedForce { get; private set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(null, 0.0f);

        public UsableHandlerInfo(Transform fireTransform, float ChargedForce)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedForce = ChargedForce;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformForceInfo(Transform fireTransform, float ChargedForce)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            this.ChargedForce = ChargedForce;
            return this;
        }
        #endregion
    }
}