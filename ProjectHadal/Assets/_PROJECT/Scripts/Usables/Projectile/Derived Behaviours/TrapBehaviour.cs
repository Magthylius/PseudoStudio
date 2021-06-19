using UnityEngine;
//using Hadal.AI;
using Magthylius.DataFunctions;
using ExitGames.Client.Photon;
using Hadal.Networking;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class TrapBehaviour : ProjectileBehaviour
    {
        [Header("AOE logic")]
        [SerializeField] private float radius = 70;
        [SerializeField] private bool isSet;
        private Collider[] detectedObjects;
        public SelfDeactivationMode selfDeactivation;

        [Header("Visual Effect")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject particleEffect;
        private Timer explodeDuration;
        private bool isExploding;

        #region Unity Lifecycle
        protected override void Start()
        {
            base.Start();
            meshRenderer = GetComponent<MeshRenderer>();
            explodeDuration = new Timer(1f);
            explodeDuration.TargetTickedEvent.AddListener(StopExplosion);
        }

        private void Update()
        {
            if(isExploding)
            {
                explodeDuration.Tick(Time.deltaTime);            
            }     
        }

        private void OnDisable()
        {
            isSet = false;

            if(meshRenderer)
            {
                meshRenderer.material.color = Color.yellow;
                return;
            }

            particleEffect.SetActive(false);
        }
        #endregion

        #region Trap Trigger Logic
        public void SubscribeModeEvent()
        {
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOn;
        }

        // Trigger locally.
        public override bool TriggerBehavior()
        {
            if (!isSet)
                return false;

            if (!IsLocal)
                return true;

            //Explode locally, check for AI
            LayerMask dectectionMask = LayerMask.GetMask("Monster"); // change this mask to AI
            detectedObjects = Physics.OverlapSphere(this.transform.position, radius, dectectionMask);
            foreach (Collider col in detectedObjects)
            {
                col.gameObject.GetComponentInChildren<IStunnable>().TryStun(5.0f);
            }
            isExploding = true;
            particleEffect.SetActive(true);

            //Send event to explode.
            Vector3 activatedSpot = gameObject.transform.position;
            object[] content = new object[] { projectileID, activatedSpot };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ACTIVATED, content);

            return true;   
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
                    isExploding = true;
                    particleEffect.SetActive(true);
                }
            }            
            return;
        }

        private void StopExplosion()
        {
            isExploding = false;
            PPhysics.OnPhysicsFinished();
        }
        #endregion

        private void OnDrawGizmosSelected() // draw circle radius for debug
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        private void ModeOn()
        {
            meshRenderer.material.color = Color.red;
            isSet = true;
        }
    }
}

