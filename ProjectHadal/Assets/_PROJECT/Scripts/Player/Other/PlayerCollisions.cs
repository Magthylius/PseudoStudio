using UnityEngine;

//Created by Jet
//edited by Jin
namespace Hadal.Controls
{
    public class PlayerCollisions : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerCameraController _cameraController;

        [Header("Layer Collisions")]
        [SerializeField] private string obstacleLayer = string.Empty;
        [SerializeField] private string interactLayer = string.Empty;

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
            LayerMask layer = LayerMask.NameToLayer(interactLayer);
            if (collision.gameObject.layer == layer.value)
            {
                if(Input.GetKeyDown(KeyCode.T))
                {
                    collision.gameObject.GetComponent<Interactable>().Interact();
                }    
            }
        }
    }
}