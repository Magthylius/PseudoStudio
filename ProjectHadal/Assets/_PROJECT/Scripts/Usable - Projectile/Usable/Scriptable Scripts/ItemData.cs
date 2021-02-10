using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public abstract class ItemData : ScriptableObject
    {
        public int ID;
        public string Name;
        public bool IsDamaging;
        public Vector3 ItemOffset;
        public GameObject ItemPrefab;
        public ProjectileData ProjectileData;
        protected GameObject spawnedObject;

        public abstract bool DoEffect(ItemHandlerInfo info);
        protected virtual GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            spawnedObject = Instantiate(ItemPrefab, position + ItemOffset, rotation, parent);
            spawnedObject.GetComponent<UsableObject>().Data = this;
            return spawnedObject;
        }
        
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}
