using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public struct UsableHandlerInfo
    {
        public Vector3 FirePoint { get; private set; }
        public Vector3 Direction { get; private set; }
        public Quaternion Orientation { get; private set; }
        public float Force { get; private set; }
        public Camera Camera { get; private set; }
        public static UsableHandlerInfo Null => new UsableHandlerInfo(null, default, null);

        public UsableHandlerInfo(Transform fireTransform, float force, Camera camera)
        {
            FirePoint = fireTransform.position;
            Direction = fireTransform.forward;
            Orientation = fireTransform.rotation;
            Force = force;
            Camera = camera;
        }

        #region Mini Builder
        public UsableHandlerInfo WithTransformInfo(Transform fireTransform)
        {
            FirePoint = fireTransform.position;
            Direction = fireTransform.forward;
            Orientation = fireTransform.rotation;
            return this;
        }
        public UsableHandlerInfo WithCamera(Camera camera)
        {
            Camera = camera;
            return this;
        }
        public UsableHandlerInfo WithForce(float force)
        {
            Force = force;
            return this;
        }
        #endregion
    }
}