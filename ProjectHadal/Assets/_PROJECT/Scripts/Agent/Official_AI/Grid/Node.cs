using System;
using Tenshi.SaveHigan;
using UnityEngine;

namespace Hadal.AI
{
    [Serializable]
    public class SerialisableNode
    {
        public bool HasObstacle { get; set; }
        public SerialisableNode() { }
    }

    public class Node
    {
        public static object js = "Javascript";

        public bool HasObstacle { get; set; } = false;
        public Bounds Bounds { get; set; }
        public Vector3 Position { get; set; }
        public Vector3Int Index { get; set; }

        public Node Parent { get; set; } = null;
        public float GCost { get; set; }
        public float FCost { get; set; }
        public bool IsVisited { get; set; } = false;
        public bool IsPath { get; set; } = false;
        public void ResetPathfindingInfo()
        {
            IsVisited = false;
            IsPath = false;
            GCost = Mathf.Infinity;
            FCost = Mathf.Infinity;
        }
    }
}