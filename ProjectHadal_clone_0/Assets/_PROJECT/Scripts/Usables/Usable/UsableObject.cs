using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public class UsableObject : MonoBehaviour, IUsable
    {
        public virtual ItemData Data { get; set; }
        public virtual bool Use(ItemHandlerInfo info) => Data.DoEffect(info);
    }
}
