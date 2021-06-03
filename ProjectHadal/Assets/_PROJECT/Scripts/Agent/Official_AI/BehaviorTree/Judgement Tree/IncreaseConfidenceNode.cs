using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class IncreaseConfidenceNode : BTNode
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

        bool IncreaseConfidence()
        {
                                        //Placeholder for now
            confidenceIncreaseValue = Random.Range( 10, 50 );
            _brain.RuntimeData.UpdateConfidenceValue(confidenceIncreaseValue);
            return true;
        }

        public override NodeState Evaluate()
        {
            if(IncreaseConfidence())
            {
                return NodeState.SUCCESS;
            }
            else
                return NodeState.FAILURE;
        }
    }
}
