using Photon.Pun;
using UnityEngine;

namespace Hadal.Player.Behaviours
{
    public class GunPartMaterial : MonoBehaviour
    {
        [SerializeField] private PhotonView pView;
        [SerializeField] private Material activeGun;
        [SerializeField] private Material inactiveGun;
        [SerializeField] private Material darkerWraith;
        private Renderer[] gunParts;

        private void Awake()
        {
            GetParts();
            InactiveMaterial();
        }

        public void ActiveMaterial() => UpdateMaterial(activeGun);
        public void InactiveMaterial() => UpdateMaterial(inactiveGun);

        private void UpdateMaterial(Material material)
        {
            Material m = material;
            if (pView.IsMine) m = darkerWraith;
            foreach (var part in gunParts)
            {
                part.material = material;
            }
        }

        private void GetParts()
        {
            if (transform.childCount == 0) return;
            gunParts = GetComponentsInChildren<Renderer>();
        }
    }
}