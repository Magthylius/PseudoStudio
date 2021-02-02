//Created by Jet
namespace Hadal.Controls
{
    public interface IUseableInput
    {
        bool FireKey1 { get; }
        bool FireKey2 { get; }
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
    public interface ILightInput
    {
        float RangeAxis { get; }
        float AngleAxis { get; }
        bool SwitchAxis { get; }
        bool SwitchTrigger { get; }
    }
}