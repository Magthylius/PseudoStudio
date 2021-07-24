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

        public PlayerController GetIsolatedPlayerIfAny(bool includePlayersInTunnel)
        {
            int livePlayerCount = 0;
            PlayerController target = null;
            foreach (var player in _detectedPlayers)
            {
                //! Do not count down players
                if (player.GetInfo.HealthManager.IsDownOrUnalive)
                    continue;
                
                //! Do not count players that are not in caverns
                if (!includePlayersInTunnel && !_brain.CavernManager.IsPlayerInValidCavern(player))
                    continue;
                
                livePlayerCount++;
                target = player;
            }
            
            if (livePlayerCount == 1)
                return target;

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

        public bool AnyDetectedPlayersInTunnels()
        {
            foreach (var player in _detectedPlayers)
            {
                if (!_brain.CavernManager.IsPlayerInValidCavern(player))
                    return true;
            }
            return false;
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