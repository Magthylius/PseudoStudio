using UnityEngine;
using Hadal.Interactables;

//Created by Jet
//edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerCollisions : MonoBehaviour, IPlayerComponent
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

        private void OnCollisionEnter(Collision collision)
        {
            LayerMask layer = LayerMask.NameToLayer(obstacleLayer);
            if (collision.gameObject.layer == layer.value)
            {
                if (_playerController.SqrSpeed >= 0.02f)
                {
                    _cameraController.ShakeCamera();
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
			if(Input.GetKeyDown(KeyCode.T))
			{
				LayerMask layer = LayerMask.NameToLayer(interactLayer);
				if (collision.gameObject.layer == layer.value)
				{
					collision.gameObject.GetComponent<Interactable>().Interact();
				}
			}
        }
    }
}