using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.Networking;
//Created by Jet, edited Jin
namespace Hadal.Usables.Projectiles
{
    public class SonicGrenadeBehaviour : ProjectileBehaviour
    {
        [Header("Stun Settings")]
        public float stunTime = 1f;

        public SelfDeactivationMode selfDeactivation;

        private float radius = 10;
        private Collider[] detectedObjects;

        //mode swapping
        public ImpulseMode impulseMode;
        [SerializeField] private bool isHighHz;
        bool triggerOnce;

        protected override void Start()
        {
            base.Start();
        }

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeSwap;
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += SonicExplode;
        }

        public void UnSubcribeModeEvent()
        {
            impulseMode.ModeSwapped -= ModeSwap;
            selfDeactivation.selfDeactivated -= SonicExplode;
        }

        //Trigger locally
        public void SonicExplode()
        {
            if (!IsLocal)
                return;

            //Scan for monster locally
            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI
            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);

            foreach (Collider col in detectedObjects)
            {
                Debug.Log("Sonic : Enemy Detected");
                col.gameObject.GetComponentInChildren<IStunnable>()?.TryStun(stunTime);
                if(isHighHz)
                { 
                    col.gameObject.GetComponent<IAmLeviathan>()?.TryToMakeRunAway();
                }
            }

            //Send event to clones
            Vector3 activatedSpot = gameObject.transform.position;
            object[] content = new object[] { projectileID, activatedSpot };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ACTIVATED, content);

            UnSubcribeModeEvent();
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
        private void ModeSwap(bool isHighHz) => this.isHighHz = isHighHz;
    }
}
