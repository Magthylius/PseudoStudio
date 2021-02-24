using UnityEngine;

namespace Hadal.Inputs
{
    //! E: Jon
    public class StandardUseableInput : IUseableInput
    {
        public bool FireKey1 => MB(0) || MBDown(0);
        public bool FireKey2 => MBDown(1);
        public bool FireKey2Held => MB(1);
        public bool FireKey2Release => MBUp(1);
        public bool EscKeyDown => KDown(KeyCode.Escape);
        public bool EscKeyUp => KUp(KeyCode.Escape);

        bool MB(int button) => Input.GetMouseButton(button);
        bool MBDown(int button) => Input.GetMouseButtonDown(button);
        bool MBUp(int button) => Input.GetMouseButtonUp(button);
        bool KDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        bool KUp(KeyCode keyCode) => Input.GetKeyUp(keyCode);
    }
}