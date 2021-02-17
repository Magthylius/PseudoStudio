using System;

namespace Hadal.Usables
{
    public interface IUsable
    {
        UsableData Data { get; }
        bool Use(UsableHandlerInfo info);
        event Action<UsableObject> OnFire;
        event Action<UsableObject> OnRestock;
        event Action<UsableObject, bool> OnSwitch;
    }
}