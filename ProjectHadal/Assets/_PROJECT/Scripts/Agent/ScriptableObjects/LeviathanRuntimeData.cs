using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using Hadal.AI.States;
using Hadal.Networking;
using Hadal.Player;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    [CreateAssetMenu(menuName = "AI/Runtime Data")]
    public class LeviathanRuntimeData : ScriptableObject
    {
        [Header("Information")]
        public LayerMask PlayerMask;
        public LayerMask ObstacleMask;
        public NavPoint navPointPrefab;
        [SerializeField] private StateMachineData machineData;

        [Header("Objectives")]
        [SerializeField, ReadOnly] MainObjective mainObjective;
        [SerializeField, ReadOnly] EngagementObjective engagementObjective;
        public MainObjective GetMainObjective => mainObjective;
        public EngagementObjective GetEngagementObjective => engagementObjective;
        public void SetMainObjective(MainObjective objective) => mainObjective = objective;
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
        public bool HasCumulativeDamageExceeded() => cumulativeDamage > cumulativeDamageThreshold;
        public void UpdateCumulativeDamageThreshold(float currentHealth)
            => cumulativeDamageThreshold = machineData.Engagement.GetAccumulatedDamageThreshold(currentHealth);

        [Header("Judgement Information")]
        [SerializeField, ReadOnly] float judgementStoptimer;
        public void TickJudgementTimer(in float deltaTime) => judgementStoptimer += deltaTime;
        public void ResetJudgementTimer() => judgementStoptimer = 0f;
        public float GetJudgementTimerValue => judgementStoptimer;

        public void Awake_Initialise()
        {
            //! Components are not initialised at this phase

            //! Information
            if (PlayerMask == default) PlayerMask = LayerMask.GetMask("LocalPlayer");
            if (ObstacleMask == default) ObstacleMask = LayerMask.GetMask("Wall");

            //! Objectives
            mainObjective = MainObjective.None;
            engagementObjective = EngagementObjective.Aggressive;

            //! Confidence
            if (machineData.RandomiseConfidenceOnAwake)
                confidence = Random.Range(machineData.MinConfidence, machineData.MaxConfidence);
            else
                confidence = machineData.InitialConfidence;
            bonusConfidence = 0;

            //! Cumulative Damage
            ResetCumulativeDamage();

            //! Judgement
            ResetJudgementTimer();
        }

        public void Start_Initialise()
        {
            //! Components should have already been initialised at this phase
        }
    }
}