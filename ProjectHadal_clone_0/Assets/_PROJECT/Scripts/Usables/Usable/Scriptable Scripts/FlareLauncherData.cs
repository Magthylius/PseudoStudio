using UnityEngine;

//Created by Jet
namespace Hadal.Usables
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