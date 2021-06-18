using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class BTRepeatUntilSuccess : BTNode
    {
        protected List<BTNode> nodes = new List<BTNode>();
        int currentLoop = -1;


        public BTRepeatUntilSuccess(List<BTNode> nodes)
        {
            this.nodes = nodes;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            foreach (var node in nodes)
            {
                while(_nodeState != NodeState.SUCCESS)
                {
                    _nodeState = NodeState.RUNNING;
                }
                _nodeState = NodeState.SUCCESS;
            }
            _nodeState = NodeState.RUNNING;
            Debug();
            return _nodeState;
        }

        public BTRepeatUntilSuccess WithDebugName(string msg)
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
