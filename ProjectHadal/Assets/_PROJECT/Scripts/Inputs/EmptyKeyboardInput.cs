using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Inputs
{
    public class EmptyKeyboardInput : IMovementInput
    {
        public float VerticalAxis => 0f;
        public bool VerticalForward => false;
        public bool VerticalBackward => false;
        public float HorizontalAxis => 0f;
        public bool HorizontalRight => false;
        public bool HorizontalLeft => false;
        public float HoverAxis => 0f;
        public bool HoverUp => false;
        public bool HoverDown => false;
        public float BoostAxis => 0f;
        public bool BoostActive => false;
    }
}
