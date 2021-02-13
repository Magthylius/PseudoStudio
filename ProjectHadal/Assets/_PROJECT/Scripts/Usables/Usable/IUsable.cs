namespace Hadal.Usables
{
    public interface IUsable
    {
        UsableData Data { get; }
        bool Use(UsableHandlerInfo info);
    }
}