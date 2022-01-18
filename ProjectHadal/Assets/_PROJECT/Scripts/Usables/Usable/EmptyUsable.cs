//Created by Jet
namespace Hadal.Usables
{
    public class EmptyUsable : UsableLauncherObject
    {
        public override UsableLauncherData Data => EmptyUsableData.Get();
        public override bool Use(UsableHandlerInfo info) => false;
    }
}
