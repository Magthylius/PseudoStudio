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
        [SerializeField] float overlapSphereDetectionRadius;
        [SerializeField] Vector3 detectionOffset;
        [SerializeField] float checkDelay;
        public int DetectedPlayersCount { get; private set; }
        public List<PlayerController> DetectedPlayers => new List<PlayerController>(_detectedPlayers);
        public float MajorDetectionRadius => overlapSphereDetectionRadius;
        private float _checkTimer;
        private List<PlayerController> _detectedPlayers;
        private AIBrain _brain;

        public UpdateMode LeviathanUpdateMode => UpdateMode.PreUpdate;

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
            _checkTimer = 0f;
            _detectedPlayers = new List<PlayerController>();
            DetectedPlayersCount = 0;

            _brain.RefreshPlayerReferences();
            PlayerController host = null; //_brain.Players.Where(p => p.HasLureLauncher).FirstOrDefault();
            if (debugEnabled)
                $"Host is not null: {host != null}".Msg();
            if (host == null) return;
            host.OnLureHasActivated += HostActivatedLure;
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

        private void SenseSurroundings()
        {
            Collider[] playerSphere = new Collider[4];
            DetectedPlayersCount = Physics.OverlapSphereNonAlloc(transform.position + detectionOffset, overlapSphereDetectionRadius, playerSphere, _brain.RuntimeData.PlayerMask);

            _detectedPlayers = playerSphere
                    .Where(c => c != null)
                    .Select(c => c.GetComponent<PlayerController>())
                    .Where(p => !p.GetInfo.HealthManager.IsDown || !p.GetInfo.HealthManager.IsUnalive)
                    .ToList();


            if (DetectedPlayersCount <= 0) _brain.CurrentTarget = null;
            else if (_brain.CurrentTarget == null) _brain.CurrentTarget = _detectedPlayers.FirstOrDefault();

            //! If already targetting, takes closest player
            if (_brain.CurrentTarget && allowSwitchTarget)
                _brain.CurrentTarget = _detectedPlayers.FirstOrDefault();

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
                    _brain.RuntimeData.SetBrainState(BrainState.Lure);
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
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position + detectionOffset, overlapSphereDetectionRadius);
        }
    }
}
