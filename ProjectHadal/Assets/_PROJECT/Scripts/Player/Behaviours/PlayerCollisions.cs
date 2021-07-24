using UnityEngine;
using Hadal.Locomotion;
using Tenshi;

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