using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class BTInverter : BTNode
    {
        //! Child node to evaluate
        protected BTNode node;

        //! The constructor requires the child node that this inverter  decorator wraps
        public BTInverter(BTNode node)
        {
            this.node = node;
        }

        //! Reports a success if the child fails and a failure if the child succeeeds. Running will report as running
        public override NodeState Evaluate(float deltaTime)
        {
            switch(node.Evaluate(deltaTime))
            {
                case NodeState.RUNNING:
                _nodeState = NodeState.RUNNING;
                break;

                case NodeState.SUCCESS:
                _nodeState = NodeState.FAILURE;
                break;

                case NodeState.FAILURE:
                _nodeState = NodeState.SUCCESS;
                break;
                default:
                    break;
            }
            return _nodeState;
        }
    }
}
