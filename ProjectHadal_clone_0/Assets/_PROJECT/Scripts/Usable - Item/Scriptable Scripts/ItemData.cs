using UnityEngine;

namespace Hadal.Equipment
{
    [CreateAssetMenu(menuName = "Items/Data")]
    public class ItemData : ScriptableObject
    {
        public int ID;
        public string Name;
        public bool IsDamaging;
        public Vector3 ItemOffset;
        public GameObject ItemPrefab;
        public GameObject ProjectilePrefab;

        public virtual GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = Instantiate(ItemPrefab, position + ItemOffset, rotation, parent);
            obj.GetComponent<UsableObject>().Data = this;
            return obj;
        }
        public virtual GameObject InstanstiateProjectile(Vector3 position, Quaternion rotation, Transform parent)
            => Instantiate(ProjectilePrefab, position, rotation, parent);
    }
}
