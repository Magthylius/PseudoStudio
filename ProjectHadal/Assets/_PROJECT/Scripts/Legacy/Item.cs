using UnityEngine;

namespace Hadal.Legacy
{
    public abstract class Item : MonoBehaviour
    {
        public ItemInfo ItemInfo;
        public GameObject itemGameObject;

        public abstract void Use();
        public virtual void SetActiveState(bool state) { }
    }
}