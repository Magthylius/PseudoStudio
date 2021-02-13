//Created by Jet
namespace Hadal.Usables
{
    public class EmptyUsable : UsableObject
    {
        public override ItemData Data => EmptyItemData.Get();
        public override bool Use(ItemHandlerInfo info) => true;
    }
}
