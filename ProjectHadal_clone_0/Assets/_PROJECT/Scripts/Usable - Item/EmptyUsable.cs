namespace Hadal.Equipment
{
    public class EmptyUsable : UsableObject
    {
        public override ItemData Data => EmptyItemData.Get();
        public override bool Use() => true;
    }
}
