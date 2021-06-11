using UnityEngine;
using Hadal.Interactables;

//Created by Jet
//edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerCollisions : MonoBehaviourDebug, IPlayerComponent
    {
        [Header("Layer Collisions")]
        [SerializeField] private string obstacleLayer = string.Empty;
        [SerializeField] private string interactLayer = string.Empty;

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
            if (_playerController.SqrSpeed >= 0.02f)
            {
                // _playerController.Mover.Speed.Normalised
                // _cameraController.ShakeCamera();
            }
            _playerController.GetInfo.Mover.Speed.Max = 10;
        }

        internal void CollisionStay(Collision collision)
        {

        }

        internal void CollisionExit(Collision collision)
        {
            _playerController.GetInfo.Mover.Speed.Max = 75;
        }

        internal void TriggerEnter(Collider collider) { }
        internal void TriggerStay(Collider collider) { }
        internal void TriggerExit(Collider collider) { }
    }
}