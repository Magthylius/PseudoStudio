using UnityEngine;

namespace Hadal.Player
{
    [CreateAssetMenu(menuName = "Player/Physic Data")]
    public class PlayerPhysicData : ScriptableObject
    {
        [SerializeField] private float mass;
        [SerializeField] private bool useGravity;
        [SerializeField] private bool isKinematic;
        [SerializeField] private PhysicMaterial physicMaterial;

        public void SetPhysicDataForController(PlayerController player)
        {
            Rigidbody rBody = player.GetInfo.Rigidbody;
            Collider collider = player.GetInfo.Collider;

            rBody.mass = mass;
            rBody.useGravity = useGravity;
            rBody.isKinematic = isKinematic;
            collider.material = physicMaterial;
        }
    }
}
