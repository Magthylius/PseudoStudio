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
        [SerializeField, Range(0f, 1f)] private float collisionSpeedThreshold;

        private PlayerController _playerController;
        private PlayerCameraController _cameraController;
        private SpeedInfo _speed;


        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _playerController = controller;
            _cameraController = info.CameraController;
            _speed = info.Mover.Speed;
        }

        internal void CollisionEnter(Collision collision)
        {
            float normSpeed = _speed.Normalised.NormaliseValue(0f, _speed.Max);
            if (normSpeed >= collisionSpeedThreshold)
                _cameraController.ShakeCamera(normSpeed);
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