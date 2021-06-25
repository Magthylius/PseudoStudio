using System.Collections;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class MushroomBehaviour : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private MeshRenderer renderersInner;
        [SerializeField] private MeshRenderer renderersOuter;

        private MaterialPropertyBlock outerBlock;
        private MaterialPropertyBlock innerBlock;

        public MeshRenderer GetInnerRenderer => renderersInner;
        public MeshRenderer GetOuterRenderer => renderersOuter;

        public void Initialise()
        {
            innerBlock = new MaterialPropertyBlock();
            outerBlock = new MaterialPropertyBlock();
            if (GetInnerRenderer != null) GetInnerRenderer.GetPropertyBlock(innerBlock);
            if (GetOuterRenderer != null) GetOuterRenderer.GetPropertyBlock(outerBlock);
        }

        public void SetMaterialEmissionData(float rate, float breathing)
        {
            innerBlock.SetFloat(EmissionRate, rate);
            innerBlock.SetFloat(EmissionBreathing, breathing);
            SetInnerBlock();
        }

        public void SetMaterialData(MushroomShaderData data)
        {
            outerBlock.SetColor(BorderColour, data.BorderColour);
            outerBlock.SetFloat(BorderPower, data.BorderPower);
            outerBlock.SetFloat(NoiseScale, data.NoiseScale);
            outerBlock.SetFloat(Alpha, data.Alpha);
            SetOuterBlock();

            innerBlock.SetColor(Colour, data.Colour);
            SetInnerBlock();
        }

        private void SetInnerBlock()
        {
            GetInnerRenderer.SetPropertyBlock(innerBlock);
        }

        private void SetOuterBlock()
        {
            GetOuterRenderer.SetPropertyBlock(outerBlock);
        }

        #region Macro Strings
        private readonly string BorderColour = "_BorderColor";
        private readonly string Colour = "_Color";
        private readonly string BorderPower = "_BorderPower";
        private readonly string NoiseScale = "_NoiseScale";
        private readonly string Alpha = "_Alpha";
        private readonly string EmissionRate = "_EmissionRate";
        private readonly string EmissionBreathing = "_EmissionBreathing";
        #endregion
    }

    [System.Serializable]
    public class MushroomShaderData
    {
        public Color BorderColour = Color.white;
        public Color Colour = Color.white;
        public float BorderPower = 0f;
        public float NoiseScale = 0f;
        public float Alpha = 0.9f;
        [MinMaxSlider(0.1f, 50f)] public Vector2 EmissionRate;
        [MinMaxSlider(0.1f, 50f)] public Vector2 EmissionBreathing;
        public void Lerp(MushroomShaderData a, MushroomShaderData b, in float percent)
        {
            BorderColour = Color.Lerp(a.BorderColour, b.BorderColour, percent);
            Colour = Color.Lerp(a.Colour, b.Colour, percent);
            BorderPower = Mathf.Lerp(a.BorderPower, b.BorderPower, percent);
            NoiseScale = Mathf.Lerp(a.NoiseScale, b.NoiseScale, percent);
            Alpha = Mathf.Lerp(a.Alpha, b.Alpha, percent);
        }
    }
}