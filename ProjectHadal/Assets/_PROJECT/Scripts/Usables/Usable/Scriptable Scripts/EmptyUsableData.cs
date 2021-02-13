using UnityEngine;

namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Empty Data")]
    public class EmptyUsableData : UsableData
    {
        public override void DoEffect(UsableHandlerInfo info) { }
        public override GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            Debug.LogWarning("Instantiated an empty usable object. Is there a missing reference?");
            return new GameObject("EmptyItem GameObject");
        }
        public static UsableData Get()
            => (UsableData)Resources.Load(PathManager.EmptyUsableDataPath);
    }
}
