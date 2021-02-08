using UnityEngine;

namespace Hadal.Equipment
{
    public class UsableObject : MonoBehaviour, IUsable
    {
        public virtual ItemData Data { get; set; }
        public virtual bool Use()
        {
            return true;
        }
    }
}
