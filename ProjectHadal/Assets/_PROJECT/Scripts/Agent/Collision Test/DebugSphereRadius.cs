using UnityEngine;

namespace Hadal.AI
{
    public class DebugSphereRadius : MonoBehaviour
    {
        public Color wireColour;
        public Transform centre;
        public float radius;

        private void OnDrawGizmos()
        {
            if (centre == null) return;
            Gizmos.color = wireColour;
            Gizmos.DrawWireSphere(centre.position, radius);
        }
    }
}
