using System;
using Hadal.AI.States;
using Tenshi;
using UnityEngine;
using Random = UnityEngine.Random;

//! C: Jet, E: Jon
namespace Hadal.AI
{
    [CreateAssetMenu(menuName = "AI/Runtime Data")]
    public class LeviathanRuntimeData : ScriptableObject
    {
        [Header("Information")]
        public string GrabbedPlayerLayer;
        public string FreePlayerLayer;
        public LayerMask PlayerMask;
        public LayerMask ObstacleMask;
        public NavPoint navPointPrefab;
        [SerializeField] private StateMachineData machineData;

        [Header("Objectives")]
        [SerializeField, ReadOnly] BrainState brainState;
        [SerializeField, ReadOnly] EngagementObjective engagementObjective;
        public BrainState GetBrainState => brainState;
        public EngagementObjective GetEngagementObjective => engagementObjective;
        public event Action<BrainState, EngagementObjective> OnAIStateChange;
        public void SetBrainState(BrainState state)
        {
            brainState = state;
            OnAIStateChange?.Invoke(brainState, engagementObjective);
        }
        public void SetEngagementObjective(EngagementObjective objective) => engagementObjective = objective;

        [Header("Confidence")]
        [SerializeField, ReadOnly] int confidence;
        [SerializeField, ReadOnly] int bonusConfidence;
        public int ActualConfidenceValue => (confidence + bonusConfidence).Clamp(machineData.MinConfidence, machineData.MaxConfidence);
        public float NormalisedConfidence => ActualConfidenceValue.NormaliseValue(machineData.MinConfidence, machineData.MaxConfidence);
        public void UpdateConfidenceValue(int change) => confidence += change;
        public void UpdateBonusConfidence(int change) => bonusConfidence += bonusConfidence;

        [Header("Cummulative Damage")]
        [SerializeField, ReadOnly] float cumulativeDamage;
        [SerializeField, ReadOnly] float cumulativeDamageThreshold;
        public void ResetCumulativeDamage() => cumulativeDamage = 0f;
        public void AddCumulativeDamage(float damage) => cumulativeDamage += damage.Abs();
        public void UpdateCumulativeDamageThreshold(float newThreshold) => cumulativeDamageThreshold = newThreshold;
        public bool HasCumulativeDamageExceeded => cumulativeDamage > cumulativeDamageThreshold;


        [Header("State Tickers")]
        [SerializeField, ReadOnly] float idleTicker = 0f;
        [SerializeField, ReadOnly] float anticipationTicker = 0f;
        [SerializeField, ReadOnly] float engagementTicker = 0f;
        [SerializeField, ReadOnly] float recoveryTicker = 0f;
        [SerializeField, ReadOnly] float cooldownTicker = 0f;

        #region Tick Functions
        public void TickIdleTicker(in float deltaTime) => TickATicker(ref idleTicker, deltaTime);
        public void ResetIdleTicker() => ResetATicker(ref idleTicker);
        public float GetIdleTicks => idleTicker;

        public void TickAnticipationTicker(in float deltaTime) => TickATicker(ref anticipationTicker, deltaTime);
        public void ResetAnticipationTicker() => ResetATicker(ref anticipationTicker);
        public float GetAnticipationTicks => anticipationTicker;

        public void TickEngagementTicker(in float deltaTime) => TickATicker(ref engagementTicker, deltaTime);
        public void ResetEngagementTicker() => ResetATicker(ref engagementTicker);
        public float GetEngagementTicks => engagementTicker;

        public bool HasJudgementTimerOfIndexExceeded(int index)
            => engagementTicker > machineData.Engagement.GetJudgementTimerThreshold(index);

        public void TickRecoveryTicker(in float deltaTime) => TickATicker(ref recoveryTicker, deltaTime);
        public void ResetRecoveryTicker() => ResetATicker(ref recoveryTicker);
        public float GetRecoveryTicks => recoveryTicker;

        public void TickCooldownTicker(in float deltaTime) => TickATicker(ref cooldownTicker, deltaTime);
        public void ResetCooldownTicker() => ResetATicker(ref cooldownTicker);
        public float GetCooldownTicks => cooldownTicker;

        void TickATicker(ref float ticker, in float deltaTime) => ticker += deltaTime;
        void ResetATicker(ref float ticker) => ticker = 0f;
        private void ResetAllTickers()
        {
            ResetIdleTicker();
            ResetAnticipationTicker();
            ResetEngagementTicker();
            ResetRecoveryTicker();
            ResetCooldownTicker();
        }

        #endregion

        public void Awake_Initialise()
        {
            //! Components are not initialised at this phase

            //! Information
            if (PlayerMask == default) PlayerMask = LayerMask.GetMask("LocalPlayer");
            if (ObstacleMask == default) ObstacleMask = LayerMask.GetMask("Wall");

            //! Objectives Reset
            SetBrainState(BrainState.None);
            SetEngagementObjective(EngagementObjective.Judgement);

            //! Confidence
            if (machineData.RandomiseConfidenceOnAwake)
                confidence = Random.Range(machineData.MinConfidence, machineData.MaxConfidence);
            else
                confidence = machineData.InitialConfidence;
            bonusConfidence = 0;

            //! Cumulative Damage
            ResetCumulativeDamage();

            //! Tickers
            ResetAllTickers();
        }

        public void Start_Initialise()
        {
            //! Components should have already been initialised at this phase
        }
    }
}