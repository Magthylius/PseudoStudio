using Hadal.Utility;
using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class StandardRotationInput : IRotationInput
    {
        private const float TriggerRange = 0.0f;

        public float XAxis => Input.GetAxis("Mouse X");
        public float YAxis => Input.GetAxis("Mouse Y");
        public float ZAxis => Input.GetAxis("ZRotate");

        public bool XTrigger => XAxis.Abs() > TriggerRange;
        public bool YTrigger => YAxis.Abs() > TriggerRange;
        public bool ZTrigger => ZAxis.Abs() > TriggerRange;
    }
}