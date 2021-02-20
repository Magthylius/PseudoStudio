using UnityEngine;

namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Usables/Empty Data")]
    public class EmptyUsableData : UsableLauncherData
    {
        public override void DoEffect(UsableHandlerInfo info) { }
        public static UsableLauncherData Get()
            => (UsableLauncherData)Resources.Load(PathManager.EmptyUsableDataPath);
    }
}
