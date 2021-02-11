using UnityEngine;

namespace Hadal.Legacy
{

    public class CameraMoveScript : MonoBehaviour
    {
        private float moveSpeed = 0.01f;
        private float x;
        private float y;
        private Vector3 rotateValue;

        void Update()
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            }

            y = Input.GetAxis("Mouse X");
            x = Input.GetAxis("Mouse Y");
            rotateValue = new Vector3(x, y * -1, 0);
            transform.eulerAngles = transform.eulerAngles - rotateValue;
        }
    }
}