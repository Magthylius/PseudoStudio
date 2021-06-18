using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hadal.Player;

namespace Hadal.AI
{
    public class AISenseDetection : MonoBehaviour, ILeviathanComponent
    {
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
            //Physics.SyncTransforms();
            //Collider[] playerSphere = Physics.OverlapSphere(transform.position + detectionOffset, overlapSphereDetectionRadius, _brain.RuntimeData.PlayerMask);
            Collider[] playerSphere = new Collider[4];
            DetectedPlayersCount = Physics.OverlapSphereNonAlloc(transform.position + detectionOffset, overlapSphereDetectionRadius, playerSphere, _brain.RuntimeData.PlayerMask);
            
            //DetectedPlayersCount = playerSphere.Length;
            _detectedPlayers = playerSphere.Where(c => c != null).Select(c => c.GetComponent<PlayerController>()).ToList();

            //Temporary
            //print("detecting: " + _detectedPlayers.FirstOrDefault());
            _brain.CurrentTarget = _detectedPlayers.FirstOrDefault();
            //if (_brain.DebugEnabled) Debug.Log("I SENSE:" + DetectedPlayersCount);
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
