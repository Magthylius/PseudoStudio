using UnityEngine;

namespace Hadal.Inputs
{
    //! Creator: Jet
    public class StandardInteractableInput : IInteractInput
    {
        public bool InteractKey => Input.GetKeyDown(KeyCode.R);
    }
}