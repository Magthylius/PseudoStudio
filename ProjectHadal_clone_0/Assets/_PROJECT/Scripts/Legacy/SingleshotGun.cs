using UnityEngine;

namespace Hadal.Legacy
{
    public class SingleshotGun : Gun
    {
        [SerializeField] Camera cam;

        public override void Use()
        {
            Shoot();
        }

        void Shoot()
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //cast iteminfo class into guninfo class to get damage variable.
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)ItemInfo).damage);

            }
        }
    }
}