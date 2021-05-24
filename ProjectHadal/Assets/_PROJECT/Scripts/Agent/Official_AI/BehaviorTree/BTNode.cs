using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    [System.Serializable]
    public abstract class BTNode
    {
        protected NodeState _nodeState;
        public NodeState nodeState { get { return nodeState; } }

        public abstract NodeState Evaluate();
    }

    public enum NodeState
    {
        RUNNING, SUCCESS, FAILURE,
    }
}