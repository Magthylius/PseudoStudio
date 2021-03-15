using UnityEngine;
using Hadal.AI;
//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class TrapBehaviour : ProjectileBehaviour
    {
        [SerializeField] private float radius = 70;
        private Collider[] detectedObjects;

        public SelfDeactivationMode selfDeactivation;

        [SerializeField] private bool isSet;

        private void OnDisable()
        {
            isSet = false;
        }

        public void SubscribeModeEvent()
        {
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOn;
        }

        public override bool TriggerBehavior()
        {
            if (!isSet)
                return false;

            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI

            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);

            foreach (Collider col in detectedObjects)
            {
                if (col.GetComponent<AIBrain>())
                {
                    col.GetComponent<AIBrain>().SetIsStunned(true); 
                }                  
            }

            PPhysics.OnPhysicsFinished();
            return true;   
        }
        private void OnDrawGizmosSelected() // draw circle radius for debug
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        private void ModeOn() => isSet = true;
    }
}

