using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.TreeNodes;

namespace Hadal.AI
{
    public class DebugNode : MonoBehaviour
    {
        [SerializeField] AIBrain brain;
        [SerializeField] AIDamageManager damageManager;
        ThreshCarriedPlayerNode testThresh; 

        // Start is called before the first frame update
        void Start()
        {
            brain = brain.gameObject.GetComponent<AIBrain>();
            damageManager = damageManager.gameObject.GetComponent<AIDamageManager>();
            testThresh = new ThreshCarriedPlayerNode(brain, damageManager);

        }

        // Update is called once per frame
        void Update()
        {
            testThresh.Evaluate(Time.deltaTime);
        }
    }
}
