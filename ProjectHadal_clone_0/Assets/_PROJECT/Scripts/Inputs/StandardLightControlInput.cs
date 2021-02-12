using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class StandardLightControlInput : ILightInput
    {
        private bool _isOn = false;

        public float RangeAxis => Input.GetAxis("LightRange");
        public float AngleAxis => Input.mouseScrollDelta.y;
        public bool SwitchTrigger => Input.GetKeyDown(KeyCode.C);
        public bool SwitchAxis
        {
            get
            {
                if (SwitchTrigger)
                {
                    _isOn = !_isOn;
                }
                return _isOn;
            }
        }
    }
}