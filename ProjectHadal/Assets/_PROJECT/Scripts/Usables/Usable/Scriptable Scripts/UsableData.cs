using Hadal.Usables.Projectiles;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public abstract class UsableData : ScriptableObject
    {
        public int ID;
        public string Name;
        public bool IsDamaging;
        public Vector3 ItemOffset;
        public GameObject UsablePrefab;
        public ProjectileData ProjectileData;
        
        public virtual void DoEffect(UsableHandlerInfo info)
        {
            var projectileObj = FlarePool.Instance.Scoop().WithGObjectSetActive(true);
            projectileObj.Data = ProjectileData;
            projectileObj.DumpEvent = (obj) => FlarePool.Instance.Dump((FlareObject) obj);
            projectileObj.gameObject.transform.position = info.FirePoint;
            projectileObj.gameObject.transform.rotation = info.Orientation;
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }


        public virtual GameObject InstanstiateItem(Vector3 position, Quaternion rotation, Transform parent)
        {
            return null;
        }
        
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}
