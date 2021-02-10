using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Items/Flare")]
    public class FlareLauncherData : ItemData
    {
        public override bool DoEffect(ItemHandlerInfo info)
        {
            return true;
        }
    }
}