using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public struct UsableHandlerInfo
    {
        public Vector3 FirePoint { get; private set; }
        public Quaternion Orientation { get; private set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(null);

        public UsableHandlerInfo(Transform fireTransform)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformInfo(Transform fireTransform)
        {
            FirePoint = fireTransform.position;
            Orientation = fireTransform.rotation;
            return this;
        }
        #endregion
    }
}