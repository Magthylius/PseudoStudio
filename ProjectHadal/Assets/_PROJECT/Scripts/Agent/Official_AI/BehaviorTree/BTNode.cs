using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    [System.Serializable]
    public class BTNode
    {
        public static bool EnableDebug = false;
        protected NodeState _nodeState;
        protected string debugName = string.Empty;
        public NodeState nodeState { get { return nodeState; } }

        public virtual NodeState Evaluate(float deltaTime) { return _nodeState; }
        public void SetDebugName(string msg) => debugName = msg;
    }

    public enum NodeState
    {
        RUNNING, SUCCESS, FAILURE,
    }
}