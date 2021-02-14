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
            projectileObj.DumpEvent += DumpProjectileMethod;
            projectileObj.SetPositionRotation(info.FirePoint, info.Orientation);
            projectileObj.WithGObjectSetActive(true);
            projectileObj.Rigidbody.AddForce(info.Direction * (info.Force * ProjectileData.Movespeed));
        }

        protected virtual void DumpProjectileMethod(ProjectileBehaviour obj) { }
        private void OnValidate()
        {
            Name = name.Replace(" Data", string.Empty);
        }
    }
}
