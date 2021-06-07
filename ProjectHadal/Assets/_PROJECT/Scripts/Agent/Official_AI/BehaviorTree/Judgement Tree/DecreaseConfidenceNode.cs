using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class DecreaseConfidenceNode : BTNode
    {
        private AIBrain _brain;
        private int confidenceDecreaseValue;


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        bool DecreaseConfidence()
        {
            //Need to change this to maybe how much damage is taken/ how many players?
            confidenceDecreaseValue = Random.Range( -10, -30 );
            _brain.RuntimeData.UpdateConfidenceValue(confidenceDecreaseValue);
            return true;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if(DecreaseConfidence())
            {
                return NodeState.SUCCESS;
            }
            else
                return NodeState.FAILURE;
        }
    }
}
