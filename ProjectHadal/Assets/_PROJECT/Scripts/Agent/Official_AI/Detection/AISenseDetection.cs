using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AISenseDetection : MonoBehaviour
    {
        [SerializeField] float overlapSphereDetectionRadius;
         [SerializeField] Vector3 detectPoint;
        public int playerAmount { get; private set; }
        //! Another way of doing this is making a big collider as a child and detect through ontriggerenter

        void Start()
        {

        }

        void Update()
        {
            SenseSurrondings();
        }

        //! We probably gonna call this once the AI sees a player to detect its surrondings how many players are there
        void SenseSurrondings()
        {
            Collider[] playerSphere = Physics.OverlapSphere(transform.position + detectPoint, overlapSphereDetectionRadius, LayerMask.GetMask("Player"));
            foreach (var player in playerSphere)
            {
                Debug.Log("MY SPIDEY SENSE IS TINGLING");
                playerAmount++;
                Debug.Log("I SENSE:" + playerAmount);
                
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position + detectPoint, overlapSphereDetectionRadius);
        }
    }
}
