using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hadal.Player;
using Photon.Pun;
using Tenshi.UnitySoku;

namespace Hadal.AI
{
    public class AISenseDetection : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField] private bool debugEnabled;

        [SerializeField] private bool allowSwitchTarget = false;
        [HideInInspector] public bool useAmbushDetection;
        [Header("Detection Settings")]
        [SerializeField] float overlapSphereDetectionRadius;
        [SerializeField] float ambushSphereDetectionRadius;
        [SerializeField] float huntSphereDetectionRadius;
        [SerializeField] Vector3 detectionOffset;
        [SerializeField] float checkDelay;
        public int DetectedPlayersCount { get; private set; }
        public List<PlayerController> DetectedPlayers => new List<PlayerController>(_detectedPlayers);
        public float MajorDetectionRadius => overlapSphereDetectionRadius;
        private float _checkTimer;
        private float _currentSphereDetectionRadius;
        private DetectionMode _detectionMode;
        private List<PlayerController> _detectedPlayers;
        private AIBrain _brain;

        public UpdateMode LeviathanUpdateMode => UpdateMode.PreUpdate;

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
            _checkTimer = 0f;
            _detectedPlayers = new List<PlayerController>();
            DetectedPlayersCount = 0;
            SetDetectionMode(DetectionMode.Normal);
        }

        public void DoUpdate(in float deltaTime)
        {
            if (TickTimer(deltaTime) <= 0)
            {
                ResetTimer();
                SenseSurroundings();
            }
        }

        public void DoFixedUpdate(in float fixedDeltaTime) { }

        public void DoLateUpdate(in float deltaTime) { }

        public void SetDetectionMode(DetectionMode mode)
        {
            _detectionMode = mode;
            UpdateDetectionSettings();
        }

        public void RequestImmediateSensing() => SenseSurroundings();

        public PlayerController GetIsolatedPlayerIfAny()
        {
            if (_detectedPlayers.Count == 1)
                return _detectedPlayers.FirstOrDefault();

            return null;
        }

        public float GetCurrentSenseDetectionRadius()
        {
            return _currentSphereDetectionRadius;
        }

        private void UpdateDetectionSettings()
        {
            switch (_detectionMode)
            {
                case DetectionMode.Normal:
                {
                    _currentSphereDetectionRadius = overlapSphereDetectionRadius;
                    return;
                }
                case DetectionMode.Ambush:
                {
                    _currentSphereDetectionRadius = ambushSphereDetectionRadius;
                    return;
                }
                case DetectionMode.Hunt:
                {
                    _currentSphereDetectionRadius = huntSphereDetectionRadius;
                    return;
                }
                default: return;
            }
        }

        private void SenseSurroundings()
        {
            Collider[] playerSphere = new Collider[4];

            DetectedPlayersCount = Physics.OverlapSphereNonAlloc(transform.position + detectionOffset, _currentSphereDetectionRadius, playerSphere, _brain.RuntimeData.PlayerMask);

            _detectedPlayers = playerSphere
                    .Where(c => c != null)
                    .Select(c => c.GetComponent<PlayerController>())
                    .Where(p => !p.GetInfo.HealthManager.IsDownOrUnalive)
                    .ToList();

            DetectedPlayersCount = _detectedPlayers.Count;

            if (DetectedPlayersCount <= 0) _brain.TrySetCurrentTarget(null);
            else if (_brain.CurrentTarget == null) _brain.TrySetCurrentTarget(_detectedPlayers.FirstOrDefault());

            //! If already targetting, takes closest player
            if (_brain.CurrentTarget && allowSwitchTarget)
                _brain.ForceSetCurrentTarget(_detectedPlayers.FirstOrDefault());

        }

        void HostActivatedLure(bool lureActivated, PlayerController actor)
        {
            bool playerInSameCavernAsAI = _brain.CavernManager.GetCavernTagOfAILocation() == _brain.CavernManager.GetCavernWithPlayerOfViewID(actor.ViewID).cavernTag;
            bool lureCriteriaMet = lureActivated && playerInSameCavernAsAI;

            if (debugEnabled)
                $"Received lure activated update, evaluating with lureActivated:{lureActivated} and playerInSameCavern:{playerInSameCavernAsAI}.".Msg();

            if (lureCriteriaMet)
            {
                if (debugEnabled)
                    $"AI should be lured.".Msg();

                NavPoint point = actor.GetComponentInChildren<NavPoint>(); //! check if actor already has a navpoint attached to it
                if (point == null || !point.IsLurePoint) // run this if there is no navpoint or it is not a lure point
                {
                    point = Instantiate(_brain.RuntimeData.navPointPrefab);
                    point.AttachTo(actor.GetTarget);
                    point.SetIsLurePoint(true);
                    SetAIToBecomeLured();
                }
                else if (point != null && point.IsLurePoint) //! point already exists && is a lure point
                {
                    SetAIToBecomeLured();
                }

                void SetAIToBecomeLured()
                {
                    _brain.NavigationHandler.SetCustomPath(point, false);
                    // _brain.RuntimeData.SetBrainState(BrainState.Lure);
                }
            }
            else if (!lureActivated) //! lure has become inactive
            {
                if (debugEnabled)
                    $"AI should no longer be lured.".Msg();

                //_brain.NavigationHandler.StopCustomPath();

            }
        }

        private float TickTimer(in float deltaTime) => _checkTimer -= deltaTime;
        private void ResetTimer() => _checkTimer = checkDelay;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(transform.position + detectionOffset, _currentSphereDetectionRadius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position + detectionOffset, overlapSphereDetectionRadius);
            }
        }

        public enum DetectionMode
        {
            Normal,
            Ambush,
            Hunt
        }
    }
}