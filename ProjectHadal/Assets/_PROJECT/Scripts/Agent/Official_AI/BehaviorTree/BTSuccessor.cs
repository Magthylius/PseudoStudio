using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class BTSuccessor : BTNode
    {
        protected List<BTNode> nodes = new List<BTNode>();
        int currentLoop = -1;


        public BTSuccessor(List<BTNode> nodes)
        {
            this.nodes = nodes;
        }

        /* If any of the children reports a failure, the successor will
        * not give a fuck. It will wait until a child succeed. */
        public override NodeState Evaluate(float deltaTime)
        {
            foreach (var node in nodes)
            {
                switch (node.Evaluate(deltaTime))
                {
                    case NodeState.SUCCESS:
                        _nodeState = NodeState.SUCCESS;
                        Debug();
                        return _nodeState;
                }
            }
            
            _nodeState = NodeState.RUNNING;
            Debug();
            return _nodeState;
        }

        public BTSuccessor WithDebugName(string msg)
        {
            debugName = msg.AddSpacesBeforeCapitalLetters(false) + "?";
            return this;
        }

        private void Debug()
        {
            if (EnableDebug)
                $"Name: {debugName}, Nodestate: {_nodeState}".Msg();
        }
    }
}
