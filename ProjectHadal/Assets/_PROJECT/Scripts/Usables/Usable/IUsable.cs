namespace Hadal.Usables
{
    public interface IUsable
    {
        ItemData Data { get; }
        bool Use(ItemHandlerInfo info);
    }
}