using UnityEngine;

namespace Hadal.Inputs
{
    //! Creator: Jet
    public class StandardInteractionInput : IInteractInput
    {
        public bool ReviveKey => false;
        public bool PickupKey => Input.GetKeyDown(KeyCode.V);
    }
}