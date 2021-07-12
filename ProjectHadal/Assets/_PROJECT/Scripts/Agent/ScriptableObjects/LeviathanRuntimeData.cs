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
        [SerializeField] private bool eggDestroyed;
        [SerializeField] private StateMachineData machineData;
        public bool IsEggDestroyed => eggDestroyed;
        public void SetIsEggDestroyed(bool statement) => eggDestroyed = statement;

        [Header("Objectives")]
        [SerializeField, ReadOnly] BrainState brainState;
        [SerializeField, ReadOnly] BrainState previousBrainState;
        [SerializeField, ReadOnly] EngagementObjective engagementObjective;
        public BrainState GetBrainState => brainState;
        public BrainState GetPreviousBrainState => previousBrainState;
        public EngagementObjective GetEngagementObjective => engagementObjective;
        public event Action<BrainState, EngagementObjective> OnAIStateChange;
        public void SetBrainState(BrainState newState)
        {
            SetPreviousBrainState(GetBrainState);
            brainState = newState;
            OnAIStateChange?.Invoke(brainState, engagementObjective);
        }
        public bool IsPreviousBrainStateEqualTo(BrainState thisState) => GetPreviousBrainState == thisState;
        private void SetPreviousBrainState(BrainState prevState)
        {
            if (GetPreviousBrainState == GetBrainState) // do not set previous state if current state is already unchanged
                return;
            
            previousBrainState = prevState;
        }
        public void SetEngagementObjective(EngagementObjective objective) => engagementObjective = objective;

        [Header("Confidence")]
        [SerializeField, ReadOnly] int confidence;
        [SerializeField, ReadOnly] int bonusConfidence;
        public int ActualConfidenceValue => (confidence + bonusConfidence).Clamp(machineData.MinConfidence, machineData.MaxConfidence);
        public float NormalisedConfidence => ActualConfidenceValue.NormaliseValue(machineData.MinConfidence, machineData.MaxConfidence);
        public void UpdateConfidenceValue(int change) => confidence += change;
        public void UpdateBonusConfidence(int change) => bonusConfidence += change;

        [Header("Cummulative Damage")]
        [SerializeField, ReadOnly] int cumulativeDamageCount;
        [SerializeField, ReadOnly] int cumulativeDamageCountThreshold;
        public void ResetCumulativeDamageCount() => cumulativeDamageCount = 0;
        public void AddDamageCount()
        {
            cumulativeDamageCount++;
            if (IsCumulativeDamageCountReached)
                OnCumulativeDamageCountReached?.Invoke();
        }
        public void UpdateCumulativeDamageCountThreshold(int newThreshold) => cumulativeDamageCountThreshold = newThreshold;
        public bool IsCumulativeDamageCountReached => cumulativeDamageCount >= cumulativeDamageCountThreshold;
        public event Action OnCumulativeDamageCountReached;


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
            SetIsEggDestroyed(false);

            //! Objectives Reset
            SetPreviousBrainState(BrainState.None);
            brainState = BrainState.None;
            SetEngagementObjective(EngagementObjective.Hunt);

            //! Confidence
            if (machineData.RandomiseConfidenceOnAwake)
                confidence = Random.Range(machineData.MinConfidence, machineData.MaxConfidence);
            else
                confidence = machineData.InitialConfidence;
            bonusConfidence = 0;

            //! Cumulative Damage
            UpdateCumulativeDamageCountThreshold(machineData.Engagement.G_DisruptionDamageCount);
            ResetCumulativeDamageCount();

            //! Tickers
            ResetAllTickers();
        }

        public void Start_Initialise()
        {
            //! Components should have already been initialised at this phase
        }
    }
}