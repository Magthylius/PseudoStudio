using UnityEngine;

namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Items/Empty Data")]
    public class EmptyItemData : ItemData
    {
        public override GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            Debug.LogWarning("Instantiated an empty item object. Is there a missing reference?");
            return new GameObject("EmptyItem GameObject");
        }
        public override GameObject InstanstiateProjectile(Vector3 position, Quaternion rotation, Transform parent)
        {
            Debug.LogWarning("Instantiated an empty projectile object. Is there a missing reference?");
            return new GameObject("EmptyProjectile GameObject");
        }

        public static ItemData Get()
            => (ItemData)Resources.Load(PathManager.EmptyItemDataPath);
    }
}
