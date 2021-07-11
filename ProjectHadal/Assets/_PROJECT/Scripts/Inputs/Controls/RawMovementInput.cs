using System;
using System.Collections.Generic;
using System.Linq;
using Tenshi.UnitySoku;
using UnityEngine;

//Created by Jet
namespace Hadal.Inputs
{
    public class RawMovementInput : IMovementInput
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

        //! Double tapping
        public float DoubleTapDetectionTime { get; set; } = 0.4f;
        public bool DoubleVerticalForward => ProcessDoubleInput(DirectionKey.Forward);
        public bool DoubleVerticalBackward => ProcessDoubleInput(DirectionKey.Backward);
        public bool DoubleHorizontalLeft => ProcessDoubleInput(DirectionKey.Left);
        public bool DoubleHorizontalRight => ProcessDoubleInput(DirectionKey.Right);
        public bool DoubleHoverUp => ProcessDoubleInput(DirectionKey.Up);
        public bool DoubleHoverDown => ProcessDoubleInput(DirectionKey.Down);
        private bool DoubleTapConditionMet => Time.time - firstPressTime < DoubleTapDetectionTime;

        private void ResetAllPreviousKeys()
        {
            int i = -1;
            var keys = AllDirectionKeys;
            while (++i < keys.Length)
            {
                keyPressedPreviously[keys[i]] = false;
                keyLetGoPreviously[keys[i]] = true;
            }
        }
        private bool ProcessDoubleInput(DirectionKey key)
        {
            AxisPair axisPair = GetAxisFromKey(key);

            if (!keyLetGoPreviously[key] && axisPair.DirectionalAxis == 0f)
                keyLetGoPreviously[key] = true;

            if (!axisPair.AxisDirectionPressed)
                return false;

            if (keyPressedPreviously[key] && keyLetGoPreviously[key])
            {
                keyPressedPreviously[key] = false;
                if (DoubleTapConditionMet) // double tap detected
                {
                    firstPressTime = 0f;
                    return true;
                }
                return false; // too late
            }

            // detect first press
            ResetAllPreviousKeys();
            keyPressedPreviously[key] = true;
            keyLetGoPreviously[key] = false;
            firstPressTime = Time.time;
            return false;
        }

        private float firstPressTime = 0f;
        private Dictionary<DirectionKey, bool> keyPressedPreviously;
        private Dictionary<DirectionKey, bool> keyLetGoPreviously;
        public RawMovementInput()
        {
            keyPressedPreviously = new Dictionary<DirectionKey, bool>();
            keyLetGoPreviously = new Dictionary<DirectionKey, bool>();
            int i = -1;
            var keys = AllDirectionKeys;
            while (++i < keys.Length)
            {
                keyPressedPreviously.Add(keys[i], false);
                keyLetGoPreviously.Add(keys[i], true);
            }
        }
        
        private AxisPair GetAxisFromKey(DirectionKey key)
        {
            return key switch
            {
                DirectionKey.Forward => new AxisPair() { DirectionalAxis = VerticalAxis, AxisDirectionPressed = VerticalForward },
                DirectionKey.Backward => new AxisPair() { DirectionalAxis = VerticalAxis, AxisDirectionPressed = VerticalBackward },
                DirectionKey.Left => new AxisPair() { DirectionalAxis = HorizontalAxis, AxisDirectionPressed = HorizontalLeft },
                DirectionKey.Right => new AxisPair() { DirectionalAxis = HorizontalAxis, AxisDirectionPressed = HorizontalRight },
                DirectionKey.Up => new AxisPair() { DirectionalAxis = HoverAxis, AxisDirectionPressed = HoverUp },
                DirectionKey.Down => new AxisPair() { DirectionalAxis = HoverAxis, AxisDirectionPressed = HoverDown },
                _ => AxisPair.Empty
            };
        }

        private static DirectionKey[] AllDirectionKeys => Enum.GetValues(typeof(DirectionKey)).Cast<DirectionKey>().ToArray();

        private struct AxisPair
        {
            public float DirectionalAxis { get; set; }
            public bool AxisDirectionPressed { get; set; }
            public static AxisPair Empty => new AxisPair() { DirectionalAxis = 0f, AxisDirectionPressed = false };
        }

        private enum DirectionKey
        {
            Forward = 0,
            Backward,
            Left,
            Right,
            Up,
            Down
        }
    }
}