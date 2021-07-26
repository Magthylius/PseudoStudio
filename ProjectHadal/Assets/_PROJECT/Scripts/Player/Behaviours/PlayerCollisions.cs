using UnityEngine;
using Hadal.Locomotion;
using Tenshi;
using Hadal.AudioSystem;

//Created by Jet
//edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerCollisions : MonoBehaviourDebug, IPlayerComponent
    {
        [Header("Layer Collisions")]
        [SerializeField, Min(0f)] private float forceSpeedThreshold;

        private PlayerController _playerController;
        private PlayerCameraController _cameraController;
        [SerializeField] private int collisionDamage;
        [SerializeField] private int collisionDamageMax;

        [SerializeField] private AudioEventData collisionSound;
        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _playerController = controller;
            _cameraController = info.CameraController;
        }

        internal void CollisionEnter(Collision collision)
        {
            float force = _playerController.GetInfo.Rigidbody.velocity.magnitude;
            
            if (force >= forceSpeedThreshold)
            {
                _cameraController.ShakeCamera(force);

                if (collisionSound)
                    collisionSound.PlayOneShot(_playerController.GetTarget);
                
                float ratio = (force / forceSpeedThreshold);
                int damage = collisionDamage * Mathf.RoundToInt(ratio);
                damage = Mathf.Clamp(damage, collisionDamage, collisionDamageMax);
                _playerController.GetInfo.HealthManager.TakeDamage(damage);
            }
        }

        internal void CollisionStay(Collision collision)
        {

        }

        internal void CollisionExit(Collision collision)
        {
            
        }

        internal void TriggerEnter(Collider collider) { }
        internal void TriggerStay(Collider collider) { }
        internal void TriggerExit(Collider collider) { }
    }
}