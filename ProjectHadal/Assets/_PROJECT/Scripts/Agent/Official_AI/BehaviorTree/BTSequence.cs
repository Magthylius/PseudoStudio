using System.Collections.Generic;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class BTSequence : BTNode
    {
        //! Children nodes that belong to this sequence
        protected List<BTNode> nodes = new List<BTNode>();
		public void AddNode(BTNode node) => nodes.Add(node);

        //! Provide an initial set of children to work
        public BTSequence(List<BTNode> nodes)
        {
            this.nodes = nodes;
        }

        //! If any child node returns a failure, the entire node fails. Whence all nodes return a success, the node reports a success.
        public override NodeState Evaluate(float deltaTime)
        {
            bool anyNodeRunning = false;
            foreach(var node in nodes)
            {
                switch(node.Evaluate(deltaTime))
                {
                    //! if node is running, means there's a process happenning.
                    case NodeState.RUNNING:
                        anyNodeRunning = true;
                        Debug();
                        return _nodeState;
                        
                    //! if node is a success, don't do anything and evaluate next child.
                    case NodeState.SUCCESS:
                        break;
                    //! if node is a failure, then break us out of the method. 
                    case NodeState.FAILURE:
                        _nodeState = NodeState.FAILURE;
                        Debug();
                        return _nodeState;
                    default:
                        break;
                }
            }
            //! If code reach here means all nodes is success, if not then it's running
            //_nodeState = anyNodeRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            Debug();
            return _nodeState;
        }

        public BTSequence WithDebugName(string msg)
        {
            debugName = msg.AddSpacesBeforeCapitalLetters(false);
            return this;
        }

        private void Debug()
        {
            if (EnableDebug)
                $"Name: {debugName}, Nodestate: {_nodeState}".Msg();
        }
    }
}
