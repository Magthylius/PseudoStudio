using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Sonic Dart")]
    public class SonicDartLauncherData : ItemData
    {
        public override bool DoEffect(ItemHandlerInfo info)
        {
            return true;
        }
    }
}