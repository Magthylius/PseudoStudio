using UnityEngine;

//Created by Jet, edited Jin
namespace Hadal.Usables.Projectiles
{
    public class SonicGrenadeBehaviour : ProjectileBehaviour
    {
        public SelfDeactivationMode selfDeactivation;

        private float radius = 10;
        private Collider[] detectedObjects;

        protected override void Start()
        {
            base.Start();
        }

        public void SubscribeModeEvent()
        {
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += SonicExplode;
        }

        private void SonicExplode()
        {
            Debug.Log("Event called");
            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI

            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);

            foreach (Collider col in detectedObjects)
            {
                Debug.Log("Enemy Detected");
            }
        }
    }
}
