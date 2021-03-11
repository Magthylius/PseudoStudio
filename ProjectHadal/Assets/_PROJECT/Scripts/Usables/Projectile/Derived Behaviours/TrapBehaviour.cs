using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class TrapBehaviour : ProjectileBehaviour
    {
        private float radius = 10;
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
                Debug.Log("Trap : Enemy Detected");
            }

            PPhysics.OnPhysicsFinished();
            return true;   
        }

        private void ModeOn() => isSet = true;
    }
}

