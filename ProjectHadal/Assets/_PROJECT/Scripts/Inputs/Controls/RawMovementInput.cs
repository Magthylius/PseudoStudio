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
        bool forwardPressedPreviously = false;
        bool backwardPressedPreviously = false;
        bool leftPressedPreviously = false;
        bool rightPressedPreviously = false;
        bool upPressedPreviously = false;
        bool downPressedPreviously = false;
        float firstPressTime = 0f;

        public float DoubleTapDetectionTime { get; set; } = 0.4f;
        public bool DoubleVerticalForward
        {
            get
            {
                if (!VerticalForward)
                    return false;

                if (forwardPressedPreviously)
                {
                    forwardPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                forwardPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }
        public bool DoubleVerticalBackward
        {
            get
            {
                if (!VerticalBackward)
                    return false;

                if (backwardPressedPreviously)
                {
                    backwardPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                backwardPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }
        public bool DoubleHorizontalLeft
        {
            get
            {
                if (!HorizontalLeft)
                    return false;

                if (leftPressedPreviously)
                {
                    leftPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                leftPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }
        public bool DoubleHorizontalRight
        {
            get
            {
                if (!HorizontalRight)
                    return false;

                if (rightPressedPreviously)
                {
                    rightPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                rightPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }
        public bool DoubleHoverUp
        {
            get
            {
                if (!HoverUp)
                    return false;

                if (upPressedPreviously)
                {
                    upPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                upPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }
        public bool DoubleHoverDown
        {
            get
            {
                if (!HoverDown)
                    return false;

                if (downPressedPreviously)
                {
                    downPressedPreviously = false;

                    //double tap detected
                    if (DoubleTapConditionMet)
                    {
                        firstPressTime = 0f;
                        return true;
                    }

                    //too late
                    return false;
                }

                //detect first press
                ResetAllPreviousKeys();
                downPressedPreviously = true;
                firstPressTime = Time.time;
                return false;
            }
        }

        private bool DoubleTapConditionMet => Time.time - firstPressTime < DoubleTapDetectionTime;
        private void ResetAllPreviousKeys()
        {
            forwardPressedPreviously = false;
            backwardPressedPreviously = false;
            leftPressedPreviously = false;
            rightPressedPreviously = false;
            upPressedPreviously = false;
            downPressedPreviously = false;
        }
    }
}