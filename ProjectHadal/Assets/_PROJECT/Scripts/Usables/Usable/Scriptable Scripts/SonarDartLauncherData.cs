using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Sonar Dart")]
    public class SonarDartLauncherData : ItemData
    {
        public override bool DoEffect(ItemHandlerInfo info)
        {
            return true;
        }
    }
}