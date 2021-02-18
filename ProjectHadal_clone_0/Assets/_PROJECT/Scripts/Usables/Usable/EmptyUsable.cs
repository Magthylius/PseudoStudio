//Created by Jet
namespace Hadal.Usables
{
    public class EmptyUsable : UsableObject
    {
        public override UsableData Data => EmptyUsableData.Get();
        public override bool Use(UsableHandlerInfo info) => false;
    }
}
