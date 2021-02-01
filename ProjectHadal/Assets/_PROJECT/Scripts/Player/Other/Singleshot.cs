using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public class Singleshot : Gun
    {
        public static int DefaultIndex = 0;
        [SerializeField] private GunPartMaterial gunPartMaterial = null;
        [SerializeField] private Camera _camera;
        [SerializeField] private string targetLayerName;
        [SerializeField] private LayerMask ignoreMask;
        [SerializeField] private Transform firePoint;

        public override void Use()
        {
            ShootWithRay();
        }
        public override void SetActiveState(bool state)
        {
            if(state)
            {
                gunPartMaterial.ActiveMaterial();
            }
            else
            {
                gunPartMaterial.InactiveMaterial();
            }
            
        }

        private void ShootWithRay()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            ray.origin = firePoint.position;

            if (Physics.Raycast(ray, out var hit, float.MaxValue, ~ignoreMask, QueryTriggerInteraction.Collide))
            {
                var trans = hit.transform;
                if (trans.gameObject.layer != LayerMask.NameToLayer(targetLayerName)) return;
                var damageable = trans.GetComponent<IDamageable>();
                if (damageable == null) return;
                damageable.TakeDamage((int)((GunInfo)ItemInfo).damage);
            }
        }
    }
}