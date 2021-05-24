using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
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
        public override NodeState Evaluate()
        {
            foreach(var node in nodes)
            {
                switch(node.Evaluate())
                {
                    case NodeState.RUNNING:
                    _nodeState = NodeState.RUNNING;
                    return _nodeState;

                    case NodeState.SUCCESS:
                    _nodeState = NodeState.SUCCESS;
                    return _nodeState;

                    case NodeState.FAILURE:
                        break;
                    default:
                        break;
                }
            }
            _nodeState = NodeState.FAILURE;
            return _nodeState;
        }
    }
}
