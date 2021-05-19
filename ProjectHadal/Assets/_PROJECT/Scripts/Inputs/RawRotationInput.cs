using Hadal.Utility;
using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class RawRotationInput : IRotationInput
    {
        private const float TriggerRange = 0.0f;

        public float XAxis => Input.GetAxisRaw("Mouse X");
        public float YAxis => Input.GetAxisRaw("Mouse Y");
        public float ZAxis => Input.GetAxisRaw("ZRotate");

        public bool XTrigger => Mathf.Abs(XAxis) > TriggerRange;
        public bool YTrigger => Mathf.Abs(YAxis) > TriggerRange;
        public bool ZTrigger => Mathf.Abs(ZAxis) > TriggerRange;
        public Vector3 AllInput => new Vector3(XAxis, YAxis, ZAxis);
    }
}