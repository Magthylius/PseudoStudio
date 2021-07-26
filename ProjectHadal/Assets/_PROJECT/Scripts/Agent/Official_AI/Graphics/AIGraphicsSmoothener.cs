using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Graphics
{
    public class AIGraphicsSmoothener : MonoBehaviour
    {
        [SerializeField] private float maxDistanceLerp = 10f;
        [SerializeField] private float maxRotationLerp = 10f;
        private AIBrain brain;
		
        void OnEnable()
        {
            StartCoroutine(TryLinkAIBrain());
        }
        
        void FixedUpdate()
        {
			if (brain == null) return;
            transform.position = Vector3.Lerp(transform.position, brain.transform.position, maxDistanceLerp * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, brain.transform.rotation, maxRotationLerp * Time.deltaTime);
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
