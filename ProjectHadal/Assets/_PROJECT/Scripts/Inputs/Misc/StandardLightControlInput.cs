using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class StandardLightControlInput : ILightInput
    {
        private bool _isOn = false;
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

        public void Toggle()
        {
            _isOn = !_isOn;
        }
    }
}