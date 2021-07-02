using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Graphics
{
    public class AIEmissiveColor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Material leviathanBody;
        
        [SerializeField] private float transitionSpeed;
        private float percent;
        private AIBrain brain;

        [Header("Color")]
        [ColorUsageAttribute(true, true)]
        public Color anticipationStateColor;
        [ColorUsageAttribute(true, true)]
        public Color judgementStateColor;

        void Start()
        {
            brain = FindObjectOfType<AIBrain>();
            brain.JudgementPhaseEvent += JudgementColor;
        }
        public void JudgementColor(bool isStarting)
        {
            if(isStarting)
            {
                StartCoroutine(AIColorLerp(judgementStateColor));
                //Debug.Log("judgementstartcolor");
            }
            else
            {
                StartCoroutine(AIColorLerp(anticipationStateColor));
                //Debug.Log("judgementendcolor");
            }
                
        }

        IEnumerator AIColorLerp(Color targetColor)
        {
            while (percent < 1f)
            {
                percent += Time.deltaTime * transitionSpeed;
                leviathanBody.SetColor("_EmissionColor", Color.Lerp(leviathanBody.GetColor("_EmissionColor"), targetColor, percent));
                yield return null;
            }
        }

        void OnDestroy()
        {
            brain.JudgementPhaseEvent -= JudgementColor;
        }
    }
}
