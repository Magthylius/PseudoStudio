using UnityEngine;

namespace Hadal.AI
{
    public class DebugBoxRadius : MonoBehaviour
    {
        public Color wireColour;
        public Transform centre;
        public float radius;

        private void OnDrawGizmos()
        {
            if (centre == null) return;
            Gizmos.color = wireColour;
            Gizmos.DrawWireCube(centre.position, new Vector3(radius, radius, radius));
        }
    }
}
