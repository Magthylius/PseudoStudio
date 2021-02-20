using UnityEngine;

namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Empty Data")]
    public class EmptyUsableData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info) { }
        public static UsableLauncherData Get()
            => (UsableLauncherData)Resources.Load(PathManager.EmptyUsableDataPath);
    }
}
