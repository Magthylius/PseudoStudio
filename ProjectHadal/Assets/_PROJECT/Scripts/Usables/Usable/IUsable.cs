using System;

namespace Hadal.Usables
{
    public interface IUsable
    {
        UsableLauncherData Data { get; }
        bool Use(UsableHandlerInfo info);
        event Action<UsableLauncherObject> OnFire;
        event Action<UsableLauncherObject> OnRestock;
        event Action<UsableLauncherObject, bool> OnSwitch;
    }
}