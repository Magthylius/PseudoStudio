using System.Collections;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class BTSelector : BTNode
    {
        //! The child nodes for this selector
        protected List<BTNode> nodes = new List<BTNode>();

        //! This constructor requires a list of child nodes to pass in
        public BTSelector(List<BTNode> nodes)
        {
            this.nodes = nodes;
        }

        /* If any of the children reports a success, the selector will
        * immediately report a success upwards. If all children fail,
        * it will report a failure instead.*/
        public override NodeState Evaluate(float deltaTime)
        {
            foreach (var node in nodes)
            {
                ExecutionOrder++;
                switch (node.Evaluate(deltaTime))
                {
                    case NodeState.RUNNING:
                        _nodeState = NodeState.RUNNING;
                        Debug();
                        return _nodeState;

                    case NodeState.SUCCESS:
                        _nodeState = NodeState.SUCCESS;
                        Debug();
                        return _nodeState;

                    case NodeState.FAILURE:
                        break;
                    default:
                        break;
                }
            }
            _nodeState = NodeState.FAILURE;
            Debug();
            return _nodeState;
        }

        private void Debug(bool includeExecutionOrder = true)
        {
            if (EnableDebug)
            {
                string msg = "";
                if (includeExecutionOrder) msg += $"{ExecutionOrder}) ";
                msg += $"Name: {debugName}, Nodestate: {_nodeState}";
                msg.Msg();
            }
        }
    }
}
