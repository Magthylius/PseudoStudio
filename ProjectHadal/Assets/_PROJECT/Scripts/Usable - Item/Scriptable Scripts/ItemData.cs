using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public abstract class ItemData : ScriptableObject
    {
        public int ID;
        public string Name;
        public int BaseDamage;
        public bool IsDamaging;
        public Vector3 ItemOffset;
        public GameObject ItemPrefab;
        public GameObject ProjectilePrefab;
        protected GameObject spawnedObject;

        public abstract bool DoEffect(ItemHandlerInfo info);
        protected virtual GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            spawnedObject = Instantiate(ItemPrefab, position + ItemOffset, rotation, parent);
            spawnedObject.GetComponent<UsableObject>().Data = this;
            return spawnedObject;
        }
        protected virtual GameObject InstanstiateProjectile(Vector3 position, Quaternion rotation, Transform parent)
            => Instantiate(ProjectilePrefab, position, rotation, parent);

        private void OnValidate()
        {
            Name = name;
        }
    }
}
