using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.Networking;
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

        //Trigger locally
        public void SonicExplode()
        {
            print("Sonic Grenade Triggered Locally.");
            //Scan for monster locally
            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI
            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);

            foreach (Collider col in detectedObjects)
            {
                Debug.Log("Sonic : Enemy Detected");
            }

            //Send event to clones
            Vector3 activatedSpot = gameObject.transform.position;
            object[] content = new object[] { projectileID, activatedSpot };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ACTIVATED, content);

            PPhysics.OnPhysicsFinished();
            return;
        }

        // Trigger network clones.
        public override void ReTriggerBehavior(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;

            if ((int)data[0] == projectileID)
            {
                if (gameObject.activeSelf)
                {
                    print("Sonic Grenade Triggered Due to Event");
                    gameObject.transform.position = (Vector3)data[1];
                    PPhysics.OnPhysicsFinished();
                }
            }
            return;
        }
      /*  private void SonicExplode()
        {
            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI

            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);

            foreach (Collider col in detectedObjects)
            {
                Debug.Log("Sonic : Enemy Detected");
            }
        }*/
    }
}
