using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Inputs
{
    public class EmptyMovementInput : IMovementInput
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

        public float DoubleTapDetectionTime { get => 0f; set => DoubleTapDetectionTime = 0f; }
        public bool DoubleVerticalForward => false;
        public bool DoubleVerticalBackward => false;
        public bool DoubleHorizontalLeft => false;
        public bool DoubleHorizontalRight => false;
        public bool DoubleHoverUp => false;
        public bool DoubleHoverDown => false;
    }
}
