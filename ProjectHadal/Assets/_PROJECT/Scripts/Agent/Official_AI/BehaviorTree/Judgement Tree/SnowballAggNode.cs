using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class SnowballAggNode : BTNode
    {

        private AIBrain _brain;
        private int confidenceIncreaseValue;


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        bool UpdateConfidence()
        {
            //_brain.UpdateConfidenceValue(confidenceIncreaseValue);
            return true;
        }

        public override NodeState Evaluate()
        {
            if(UpdateConfidence())
            {
                return NodeState.SUCCESS;
            }
            else
                return NodeState.FAILURE;
        }
    }
}
