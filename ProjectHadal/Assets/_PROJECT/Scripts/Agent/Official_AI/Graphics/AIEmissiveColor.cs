using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Hadal.Networking;
using Photon.Pun;
using UnityEngine;

namespace Hadal.AI.Graphics
{
    public class AIEmissiveColor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Renderer leviathanRenderer;

        [SerializeField] private float transitionSpeed;
        private float percent;
        private AIBrain _brain;
		private bool _onMasterClient;

        [Header("Color")]
        [ColorUsageAttribute(true, true)]
        public Color otherStateColor;
        [ColorUsageAttribute(true, true)]
        public Color judgementStateColor;
        [ColorUsageAttribute(true, true)]
        public Color ambushStateColor;

        /// <summary> Having a cached reference can help save computation cost. </summary>
        private MaterialPropertyBlock materialProp;

        private Coroutine colourRoutine;

        public void Initialise(AIBrain brain, bool onMasterClient)
        {
            _brain = brain;
			_onMasterClient = onMasterClient;
            if (_onMasterClient)
                _brain.RuntimeData.OnAIStateChange += JudgementColor;

            //! initialise property block (needs to use a renderer, since it has the functions to set up)
            materialProp = new MaterialPropertyBlock();
            if (materialProp != null) leviathanRenderer.GetPropertyBlock(materialProp);

            percent = 0f;
            sourceA = otherStateColor;
            targetB = otherStateColor;
            colourRoutine = null;
            UpdateMaterialData();
            if (!_onMasterClient)
                NetworkEventManager.Instance.AddListener(ByteEvents.AI_COLOUR_CHANGE, Receive_ChangeColour);
        }

        private void Receive_ChangeColour(EventData eventData)
        {
            var content = (object[])eventData.CustomData;
            bool judgement = (bool)content[0];
            bool ambush = (bool)content[1];
            if (colourRoutine != null)
                StopCoroutine(colourRoutine);
            colourRoutine = StartCoroutine(AIColorLerp(judgement, ambush));
        }

        public void JudgementColor(BrainState state, EngagementObjective objective)
        {
            bool judgement = state == BrainState.Judgement;
            bool ambush = state == BrainState.Ambush;
            if (colourRoutine != null)
                StopCoroutine(colourRoutine);
            colourRoutine = StartCoroutine(AIColorLerp(judgement, ambush));

            if (_onMasterClient)
            {
                object[] content = new object[] { judgement, ambush };
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_COLOUR_CHANGE, content, SendOptions.SendReliable);
            }
        }

        /// <summary>
        /// Lerps the material colour of the leviathan renderer to change its colour. Percent will up go towards 1f if isJudgement is true;
        /// percent will up go down towards 0f if isJudgement is false.
        /// </summary>
        IEnumerator AIColorLerp(bool isJudgement, bool isAmbush)
        {
            if (isAmbush) SetLerpTarget(ambushStateColor);
            else
            {
                if (isJudgement) SetLerpTarget(judgementStateColor);
                else SetLerpTarget(otherStateColor);
            }

            percent = 0f;
            while (percent < 1f)
            {
                percent += _brain.DeltaTime * transitionSpeed;
                UpdateMaterialData();
                yield return null;
            }
            percent = 1f;
            UpdateMaterialData();
            colourRoutine = null;
        }

        private void UpdateMaterialData()
        {
            materialProp.SetColor("_EmissionColor", LerpColourValue(percent));
            leviathanRenderer.SetPropertyBlock(materialProp);
        }

        private Color sourceA;
        private Color targetB;
        private void SetLerpTarget(Color b)
        {
            if (sourceA != b) sourceA = targetB;
            targetB = b;
        }
        private Color LerpColourValue(in float percent) => Color.Lerp(sourceA, targetB, percent);

        void OnDestroy()
        {
            if (_onMasterClient)
                _brain.RuntimeData.OnAIStateChange -= JudgementColor;
        }
    }
}
