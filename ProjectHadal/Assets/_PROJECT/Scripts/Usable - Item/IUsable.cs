namespace Hadal.Equipment
{
    public interface IUsable
    {
        ItemData Data { get; }
        bool Use(ItemHandlerInfo info);
    }
}