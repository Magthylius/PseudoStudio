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

        //Ambush Pulse
        Color colorEnd;
        Color colorStart;
        [SerializeField] private float ambushColorRate;
        [SerializeField] private float anticipationColorRate;
        private Coroutine ambushPulseRoutine;
        private Coroutine anticipationPulseRoutine;

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

            colorEnd = ambushStateColor;
            colorStart = otherStateColor;
        }

        private void Receive_ChangeColour(EventData eventData)
        {
            var content = (object[])eventData.CustomData;
            bool judgement = (bool)content[0];
            bool ambush = (bool)content[1];
            bool anticipation = (bool)content[2];
            if (colourRoutine != null)
                StopCoroutine(colourRoutine);
            colourRoutine = StartCoroutine(AIColorLerp(judgement, ambush, anticipation));
        }

        public void JudgementColor(BrainState state, EngagementObjective objective)
        {
            bool judgement = state == BrainState.Judgement;
            bool ambush = state == BrainState.Ambush;
            bool anticipation = state == BrainState.Anticipation;
            if (colourRoutine != null)
                StopCoroutine(colourRoutine);
            colourRoutine = StartCoroutine(AIColorLerp(judgement, ambush, anticipation));

            if (_onMasterClient)
            {
                object[] content = new object[] { judgement, ambush, anticipation };
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_COLOUR_CHANGE, content, SendOptions.SendReliable);
            }
        }

        /// <summary>
        /// Lerps the material colour of the leviathan renderer to change its colour. Percent will up go towards 1f if isJudgement is true;
        /// percent will up go down towards 0f if isJudgement is false.
        /// </summary>
        IEnumerator AIColorLerp(bool isJudgement, bool isAmbush, bool isAnticipation)
        {
            if (isAmbush) SetLerpTarget(ambushStateColor);
            else
            {
                if (ambushPulseRoutine != null)
                    StopCoroutine(ambushPulseRoutine);

                if (anticipationPulseRoutine != null)
                    StopCoroutine(anticipationPulseRoutine);

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

            if (isAmbush)
            {
                ambushPulseRoutine = StartCoroutine(AmbushStateColorPulse(isAmbush));
            }

            if (isAnticipation)
            {
                anticipationPulseRoutine = StartCoroutine(AnticipationStateColorPulse(isAnticipation));
            }

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

        public void Death()
        {
            if (ambushPulseRoutine != null)
                StopCoroutine(ambushPulseRoutine);

            if (anticipationPulseRoutine != null)
                StopCoroutine(anticipationPulseRoutine);

            if (colourRoutine != null)
                StopCoroutine(colourRoutine);

            materialProp.SetColor("_EmissionColor", Color.black);
            leviathanRenderer.SetPropertyBlock(materialProp);
        }

        IEnumerator AmbushStateColorPulse(bool isAmbush)
        {
            float time = 0;
            while (isAmbush)
            {
                time += Time.deltaTime * ambushColorRate;
                Color lerpColor = Color.Lerp(colorStart, colorEnd, Mathf.PingPong(time, 1));
                materialProp.SetColor("_EmissionColor", lerpColor);
                leviathanRenderer.SetPropertyBlock(materialProp);
                yield return null;
            }
        }


        IEnumerator AnticipationStateColorPulse(bool isAnticipation)
        {
            float time = 0;
            while (isAnticipation)
            {
                time += Time.deltaTime * anticipationColorRate;
                Color lerpColor = Color.Lerp(colorStart, colorEnd, Mathf.PingPong(time, 1.5f));
                materialProp.SetColor("_EmissionColor", lerpColor);
                leviathanRenderer.SetPropertyBlock(materialProp);
                yield return null;
            }
        }
    }
}