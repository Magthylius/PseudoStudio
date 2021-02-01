using UnityEngine;

namespace Hadal.Controls
{
    public class StandardUseableInput : IUseableInput
    {
        public bool FireKey1 => Input.GetMouseButtonDown(0);
        public bool FireKey2 => Input.GetMouseButtonDown(1);
    }
}