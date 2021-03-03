using UnityEngine;

namespace Hadal.AI
{
    public class Node
    {
        public bool HasObstacle { get; set; } = false;
        public Bounds Bounds { get; set; }
        public Vector3 Position { get; set; }
        public Vector3Int Index { get; set; }

        public Node Parent { get; set; } = null;
        public float GCost { get; set; }
        public float FCost { get; set; }
        public void ResetCosts()
        {
            GCost = Mathf.Infinity;
            FCost = Mathf.Infinity;
        }
    }
}