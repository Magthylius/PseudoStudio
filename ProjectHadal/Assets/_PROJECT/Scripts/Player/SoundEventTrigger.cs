// Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class SoundEventTrigger : MonoBehaviour
    {
        public float radius = 10; // radius duhh
        public bool checkObstacles = false; // if true, raycast to check for obstacles 
        public bool constantPing = false; // if true, ping constantly
        Collider[] detectedObjects; // for storing nearby colliders

        void Start()
        {
        
        }

        void Update()
        {
            if(constantPing)
                SoundPing();
        }

        public void SoundPing() // ping for nearby colliders
        {
            detectedObjects = Physics.OverlapSphere(this.transform.position, radius);

            foreach(Collider col in detectedObjects)
            {
                if(col.gameObject.tag == "Player" && checkObstacles)
                {
                    CheckObstacles(col.gameObject.transform);
                }
                else if(col.gameObject.tag == "AI")
                {
                    AIDetected();
                }
            }
        }

        private void CheckObstacles(Transform transform) // Raycast to check for obstacles
        {
            RaycastHit hit;

            Vector3 dir = (this.transform.position - transform.position).normalized;

            if (Physics.Raycast(this.transform.position, dir, out hit))
            {
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
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
