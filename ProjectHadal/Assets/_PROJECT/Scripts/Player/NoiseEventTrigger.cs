// Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class NoiseEventTrigger : MonoBehaviour
    {
        // <Summary>
        // Add this to things you want the AI to "hear"
        // Add to player 
        public float radius = 10; // radius duhhhhh
        public float radiusAdjustRate = 1; // 

        private float currentRadius;

        [SerializeField] bool checkObstacles = false; // if true, raycast to check for obstacles 
        [SerializeField] bool constantPing = false; // if true, ping constantly
        Collider[] detectedObjects; // for storing nearby colliders

        void Start()
        {
            currentRadius = radius;
        }

        void FixedUpdate()
        {
            if(constantPing)
            {
                AdjustRadius();
                NoisePing();
            }
        }

        public void SetRadius(float newRadius) // set new radius
        {
            radius = newRadius;
        }

        private void AdjustRadius() // if you want the radius to change slowly
        {
            if (currentRadius < radius)
                currentRadius += radiusAdjustRate * Time.deltaTime;
            else if (currentRadius > radius)
                currentRadius -= radiusAdjustRate * Time.deltaTime;
        }

        public void NoisePing() // ping for nearby colliders
        {
            LayerMask dectectionMask = LayerMask.GetMask("Player"); // change this mask to AI

            detectedObjects = Physics.OverlapSphere(this.transform.position, currentRadius, dectectionMask);

            foreach(Collider col in detectedObjects)
            {
                if(checkObstacles)
                    CheckObstacles(col.gameObject.transform);
                else
                    AIDetected();
            }
        }

        private void CheckObstacles(Transform transform) // Raycast to check for obstacles
        {
            RaycastHit hit;

            Vector3 dir = (transform.position - this.transform.position).normalized;

            if (Physics.Raycast(this.transform.position, dir, out hit))
            {
                Debug.DrawLine(this.transform.position, transform.position);
                Debug.Log(hit.transform.name);
                AIDetected();
            }
        }

        private void AIDetected() // On detection trigger AIEvent
        {
            //AIEventManager.instance.SoundEventTrigger();
        }

        private void OnDrawGizmosSelected() // draw circle radius for debug
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}
