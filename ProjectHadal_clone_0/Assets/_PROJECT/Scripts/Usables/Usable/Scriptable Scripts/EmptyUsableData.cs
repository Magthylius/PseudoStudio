using UnityEngine;

namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Empty Data")]
    public class EmptyUsableData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info) { }
        public static UsableData Get()
            => (UsableData)Resources.Load(PathManager.EmptyUsableDataPath);
    }
}
