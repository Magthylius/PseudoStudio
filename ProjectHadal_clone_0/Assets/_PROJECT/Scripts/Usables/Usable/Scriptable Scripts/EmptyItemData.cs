using UnityEngine;

namespace Hadal.Usables
{
    [CreateAssetMenu(menuName = "Items/Empty Data")]
    public class EmptyItemData : ItemData
    {
        public override bool DoEffect(ItemHandlerInfo info) => false;
        protected override GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            Debug.LogWarning("Instantiated an empty item object. Is there a missing reference?");
            return new GameObject("EmptyItem GameObject");
        }
        public static ItemData Get()
            => (ItemData)Resources.Load(PathManager.EmptyItemDataPath);
    }
}
