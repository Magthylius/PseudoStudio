using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AISenseDetection : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField] float overlapSphereDetectionRadius;
         [SerializeField] Vector3 detectPoint;
        public int DetectedPlayersCount { get; private set; }
        
        public UpdateMode LeviathanUpdateMode => UpdateMode.PreUpdate;
        
        public void Initialise(AIBrain brain)
        {
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

        //! Another way of doing this is making a big collider as a child and detect through ontriggerenter
        //! We probably gonna call this once the AI sees a player to detect its surrondings how many players are there
        private void SenseSurrondings()
        {
            Collider[] playerSphere = Physics.OverlapSphere(transform.position + detectPoint, overlapSphereDetectionRadius, LayerMask.GetMask("Player"));
            foreach (var player in playerSphere)
            {
                Debug.Log("MY SPIDEY SENSE IS TINGLING");
                DetectedPlayersCount++;
                Debug.Log("I SENSE:" + DetectedPlayersCount);
                
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position + detectPoint, overlapSphereDetectionRadius);
        }
    }
}
