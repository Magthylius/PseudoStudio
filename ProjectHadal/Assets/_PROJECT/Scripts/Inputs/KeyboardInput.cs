using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class KeyboardMoveInput : IMovementInput
    {
        public float VerticalAxis => Input.GetAxis("Vertical");
        public bool VerticalForward => VerticalAxis >= float.Epsilon;
        public bool VerticalBackward => VerticalAxis <= -float.Epsilon;
        public float HorizontalAxis => Input.GetAxis("Horizontal");
        public bool HorizontalRight => HorizontalAxis >= float.Epsilon;
        public bool HorizontalLeft => HorizontalAxis <= -float.Epsilon;
        public float HoverAxis => Input.GetAxis("Hover");
        public bool HoverUp => HoverAxis >= float.Epsilon;
        public bool HoverDown => HoverAxis <= -float.Epsilon;
        public float BoostAxis => Input.GetAxis("Boost");
        public bool BoostActive => BoostAxis >= float.Epsilon;
    }
}