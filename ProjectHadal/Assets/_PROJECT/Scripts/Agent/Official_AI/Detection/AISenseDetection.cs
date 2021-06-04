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
        [SerializeField] Vector3 detectPoint;
        public int DetectedPlayersCount { get; private set; }
        private AIBrain _brain;


        public UpdateMode LeviathanUpdateMode => UpdateMode.PreUpdate;

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
        }

        public void DoUpdate(in float deltaTime)
        {
            SenseSurrondings();
        }

        public void DoFixedUpdate(in float fixedDeltaTime)
        {
        }

        public void DoLateUpdate(in float deltaTime)
        {
        }

        // void ConvertTransformToGO()
        // {
        //     //players = _brain.PlayerTransforms.Select(player => player.gameObject).ToArray();
        // }

        //! Another way of doing this is making a big collider as a child and detect through ontriggerenter
        //! We probably gonna call this once the AI sees a player to detect its surrondings how many players are there
        private void SenseSurrondings()
        {
            Collider[] playerSphere = Physics.OverlapSphere(transform.position + detectPoint, overlapSphereDetectionRadius, _brain.RuntimeData.PlayerMask);
            if (_brain.DebugEnabled) Debug.Log("MY SPIDEY SENSE IS TINGLING");
            DetectedPlayersCount = playerSphere.Length;
            if (_brain.DebugEnabled) Debug.Log("I SENSE:" + DetectedPlayersCount);
            
            if (DetectedPlayersCount > 0)
            {
                _brain.CurrentTarget = playerSphere.First().GetComponent<PlayerController>();
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position + detectPoint, overlapSphereDetectionRadius);
        }
    }
}
