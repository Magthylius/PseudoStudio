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
            LayerMask layer = LayerMask.NameToLayer(obstacleLayer);
            DebugLog(layer.ToString());
            if (collision.gameObject.layer == layer.value)
            {
                if (_playerController.SqrSpeed >= 0.02f)
                {
                    _cameraController.ShakeCamera();
                }
            }
        }

        internal void CollisionStay(Collision collision)
        {
			if(Input.GetKeyDown(KeyCode.T))
			{
				LayerMask layer = LayerMask.NameToLayer(interactLayer);
				if (collision.gameObject.layer == layer.value)
				{
					collision.gameObject.GetComponent<Interactable>().Interact();
				}
			}

            LayerMask ObstacleLayer = LayerMask.NameToLayer(obstacleLayer);
            if(collision.gameObject.layer == ObstacleLayer.value)
            {

            }
        }

        internal void CollisionExit(Collision collision)
        {

        }

        internal void TriggerEnter(Collider collider) { }
        internal void TriggerStay(Collider collider) { }
        internal void TriggerExit(Collider collider) { }
    }
}