//Created by Jet
using UnityEngine;

namespace Hadal.Inputs
{
    public interface IUseableInput
    {
        bool FireKeyTorpedo { get; }
        bool FireKeyUtility { get; }
        bool FireKeyUtilityHeld { get; }
        bool FireKeyUtilityRelease { get; }
        bool FireKeyQuickFlare { get; }
        bool EscKeyDown { get; }
        bool EscKeyUp { get; }
    }
    public interface IEquipmentInput
    {
        bool SlotIndex(int index);
    }
    public interface IRotationInput
    {
        float XAxis { get; }
        bool XTrigger { get; }
        float YAxis { get; }
        bool YTrigger { get; }
        float ZAxis { get; }
        bool ZTrigger { get; }
        Vector3 AllInput { get; }
        Vector3 AllInputClamped(float min, float max);
    }
    public interface IMovementInput
    {
        float VerticalAxis { get; }
        bool VerticalForward { get; }
        bool VerticalBackward { get; }
        float HorizontalAxis { get; }
        bool HorizontalLeft { get; }
        bool HorizontalRight { get; }
        float HoverAxis { get; }
        bool HoverUp { get; }
        bool HoverDown { get; }
        float BoostAxis { get; }
        bool BoostActive { get; }
    }
    public interface IInteractInput
    {
        bool InteractKey { get; }
    }
    public interface ILightInput
    {
        bool SwitchAxis { get; }
        bool SwitchTrigger { get; }
        void Toggle();
    }
}