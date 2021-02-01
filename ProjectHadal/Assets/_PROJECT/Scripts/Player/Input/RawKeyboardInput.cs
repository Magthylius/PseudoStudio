using UnityEngine;

//Created by Jet
namespace Hadal.Controls
{
    public class RawKeyboardInput : IMovementInput
    {
        public float VerticalAxis => Input.GetAxisRaw("Vertical");
        public bool VerticalForward => VerticalAxis >= float.Epsilon;
        public bool VerticalBackward => VerticalAxis <= -float.Epsilon;
        public float HorizontalAxis => Input.GetAxisRaw("Horizontal");
        public bool HorizontalRight => HorizontalAxis >= float.Epsilon;
        public bool HorizontalLeft => HorizontalAxis <= -float.Epsilon;
        public float HoverAxis => Input.GetAxisRaw("Hover");
        public bool HoverUp => HoverAxis >= float.Epsilon;
        public bool HoverDown => HoverAxis <= -float.Epsilon;
        public float BoostAxis => Input.GetAxisRaw("Boost");
        public bool BoostActive => BoostAxis >= float.Epsilon;
    }
}