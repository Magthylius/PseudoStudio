using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AIGraphicLookAt : MonoBehaviour
    {
        [SerializeField] private float maxDistanceDelta = 10f;
        private AIBrain brain;
        
        void OnEnable()
        {
            StartCoroutine(TryLinkAIBrain());
        }
        
        void FixedUpdate()
        {
            transform.LookAt(brain.transform);
            Vector3 targetPos = Vector3.MoveTowards(transform.position, brain.transform.position, maxDistanceDelta);
            Quaternion targetRot = transform.rotation.SetLookRotation(brain.transform.position);
        }

        IEnumerator TryLinkAIBrain()
        {
            while (brain == null)
            {
                brain = FindObjectOfType<AIBrain>();
                yield return null;
            }

        }
    }
}
