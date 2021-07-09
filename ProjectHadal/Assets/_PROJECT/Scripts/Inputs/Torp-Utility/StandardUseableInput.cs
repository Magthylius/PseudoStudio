using UnityEngine;

namespace Hadal.Inputs
{
    //! E: Jon
    public class StandardUseableInput : IUseableInput
    {
        public bool FireKeyTorpedo => MB(0);
        public bool FireKeyTorpedoDown => MBDown(0);
        public bool FireKeyTorpedoRelease => MBUp(0);
        public bool FireKeyUtility => KDown(KeyCode.R);
        public bool FireKeyUtilityHeld => K(KeyCode.R);
        public bool FireKeyUtilityRelease => KUp(KeyCode.R);
        public bool FireKeyQuickFlare => KDown(KeyCode.F);
        public bool FireKeyQuickHarpoon => MBDown(1);
        public bool EscKeyDown => KDown(KeyCode.Escape);
        public bool EscKeyUp => KUp(KeyCode.Escape);
        public bool TabKeyDown => KDown(KeyCode.Tab);
        public bool TabKeyUp => KUp(KeyCode.Tab);

        bool MB(int button) => Input.GetMouseButton(button);
        bool MBDown(int button) => Input.GetMouseButtonDown(button);
        bool MBUp(int button) => Input.GetMouseButtonUp(button);
        bool K(KeyCode keyCode) => Input.GetKey(keyCode);
        bool KDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        bool KUp(KeyCode keyCode) => Input.GetKeyUp(keyCode);
    }
}