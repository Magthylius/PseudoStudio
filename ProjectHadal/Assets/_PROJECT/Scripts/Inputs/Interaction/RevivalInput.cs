using UnityEngine;

namespace Hadal.Inputs
{
    //! Creator: Jet
    public class ReviveInteractionInput : IInteractInput
    {
        public bool InteractKey => Input.GetKeyDown(KeyCode.R);
    }
}