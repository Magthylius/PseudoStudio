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

        // Start is called before the first frame update
        void Start()
        {
            brain = brain.gameObject.GetComponent<AIBrain>();
            damageManager = damageManager.gameObject.GetComponent<AIDamageManager>();
            ThreshCarriedPlayerNode testThresh = new ThreshCarriedPlayerNode(brain, damageManager);
            testThresh.Evaluate(Time.deltaTime);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
