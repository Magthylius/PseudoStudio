using UnityEngine;

//Created by Jet
namespace Hadal.Debugging
{
    using PlayerController = Controls.PlayerController;

    public class DebugScript : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private Vector3 direction;
        [SerializeField] private float speed;

        private void Update()
        {
            if (controller == null) return;
            if (Input.GetKey(KeyCode.V))
            {
                controller.AddVelocity(speed, direction);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                controller.transform.position = new Vector3(70, 20, 0);
            }
        }
    }
}