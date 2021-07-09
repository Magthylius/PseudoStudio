using UnityEngine;

namespace Hadal.Inputs
{
    //! E: Jon
    public class StandardUseableInput : IUseableInput
    {
        public bool FireKeyTorpedo => MB(0);
        public bool FireKeyTorpedoDown => MBDown(0);
        public bool FireKeyTorpedoRelease => MBUp(0);
        public bool FireKeyUtility => MBDown(1);
        public bool FireKeyUtilityHeld => MB(1);
        public bool FireKeyUtilityRelease => MBUp(1);
        public bool FireKeyQuickFlare => KDown(KeyCode.F);
        public bool EscKeyDown => KDown(KeyCode.Escape);
        public bool EscKeyUp => KUp(KeyCode.Escape);
        public bool TabKeyDown => KDown(KeyCode.Tab);
        public bool TabKeyUp => KUp(KeyCode.Tab);

        bool MB(int button) => Input.GetMouseButton(button);
        bool MBDown(int button) => Input.GetMouseButtonDown(button);
        bool MBUp(int button) => Input.GetMouseButtonUp(button);
        bool KDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        bool KUp(KeyCode keyCode) => Input.GetKeyUp(keyCode);
    }
}