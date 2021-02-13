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

        public bool XTrigger => XAxis.Abs() > TriggerRange;
        public bool YTrigger => YAxis.Abs() > TriggerRange;
        public bool ZTrigger => ZAxis.Abs() > TriggerRange;
    }
}