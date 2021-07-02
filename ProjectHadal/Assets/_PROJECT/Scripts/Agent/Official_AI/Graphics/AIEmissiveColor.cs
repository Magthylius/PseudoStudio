using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Graphics
{
    public class AIEmissiveColor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Material leviathanBody;
        [SerializeField] private MeshRenderer leviathanRenderer;
        
        [SerializeField] private float transitionSpeed;
        private float percent;
        private AIBrain brain;

        [Header("Color")]
        [ColorUsageAttribute(true, true)]
        public Color anticipationStateColor;
        [ColorUsageAttribute(true, true)]
        public Color judgementStateColor;

        /// <summary> Having a cached reference can help save computation cost. </summary>
        private MaterialPropertyBlock materialProp;

        void Start()
        {
            brain = FindObjectOfType<AIBrain>();
            brain.JudgementPhaseEvent += JudgementColor;

            //! initialise property block (needs to use a renderer, since it has the functions to set up)
            materialProp = new MaterialPropertyBlock();
            if (materialProp != null) leviathanRenderer.GetPropertyBlock(materialProp);
        }
        public void JudgementColor(bool isStarting)
        {
            StopAllCoroutines(); //! stopping any running coroutines so it will never run more than once per event call
            StartCoroutine(AIColorLerp(isStarting));
        }

        /// <summary>
        /// Lerps the material colour of the leviathan renderer to change its colour. Percent will up go towards 1f if isJudgement is true;
        /// percent will up go down towards 0f if isJudgement is false.
        /// </summary>
        IEnumerator AIColorLerp(bool isJudgement)
        {
            if (isJudgement)
            {
                //Debug.Log("judgementstartcolor");
                while (percent < 1f)
                {
                    percent += brain.DeltaTime * transitionSpeed;
                    UpdateMaterialData();
                    yield return null;
                }
                percent = 1f;
            }
            else
            {
                //Debug.Log("judgementendcolor");
                while (percent > 0f)
                {
                    percent -= brain.DeltaTime * transitionSpeed;
                    UpdateMaterialData();
                    yield return null;
                }
                percent = 0f;
            }

            UpdateMaterialData();

            // while (percent < 1f)
            // {
            //     percent += Time.deltaTime * transitionSpeed;
            //     leviathanBody.SetColor("_EmissionColor", Color.Lerp(leviathanBody.GetColor("_EmissionColor"), targetColor, percent));
            //     yield return null;
            // }
        }

        private void UpdateMaterialData()
        {
            materialProp.SetColor("_EmissionColor", LerpColourValue(percent));
            leviathanRenderer.SetPropertyBlock(materialProp);
        }

        private Color LerpColourValue(in float percent) => Color.Lerp(anticipationStateColor, judgementStateColor, percent);

        void OnDestroy()
        {
            brain.JudgementPhaseEvent -= JudgementColor;
        }
    }
}
