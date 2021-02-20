using UnityEngine;

namespace Hadal.Inputs
{
    public class StandardUseableInput : IUseableInput
    {
        public bool FireKey1 => Input.GetMouseButton(0) || Input.GetMouseButtonDown(0);
        public bool FireKey2 => Input.GetKeyDown(KeyCode.G);
    }
}