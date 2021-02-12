using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class NetworkKeyboardInput : IMovementInput
    {
        public float VerticalAxis { get; set; }
        public bool VerticalForward => VerticalAxis >= float.Epsilon;
        public bool VerticalBackward => VerticalAxis <= -float.Epsilon;
        public float HorizontalAxis { get; set; }
        public bool HorizontalRight => HorizontalAxis >= float.Epsilon;
        public bool HorizontalLeft => HorizontalAxis <= -float.Epsilon;
        public float HoverAxis { get; set; }
        public bool HoverUp => HoverAxis >= float.Epsilon;
        public bool HoverDown => HoverAxis <= -float.Epsilon;
        public float BoostAxis { get; set; }
        public bool BoostActive => BoostAxis >= float.Epsilon;

        public NetworkKeyboardInput()
        {
            ResetAllAxis();
        }

        public void ResetAllAxis()
        {
            VerticalAxis = 0.0f;
            HorizontalAxis = 0.0f;
            HoverAxis = 0.0f;
            BoostAxis = 0.0f;
        }
    }
}