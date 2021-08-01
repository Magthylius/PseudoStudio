using System;
using UnityEngine;
//using Hadal.AI;
using Magthylius.DataFunctions;
using ExitGames.Client.Photon;
using Hadal.Networking;
using Hadal.AudioSystem;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class TrapBehaviour : ProjectileBehaviour
    {
        [SerializeField] private AudioEventData readyAudio;

        [Header("Stun Settings")] 
        public float stunTime = 5f;

        [Header("AOE logic")]
        private Timer stunTimer;
        [SerializeField] private float stunInterval;
        [SerializeField] private float radius = 70;
        [SerializeField] private bool isSet;
        private Collider[] detectedObjects;
        public SelfDeactivationMode selfDeactivation;

        [Header("Visual Effect")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject explodeEffect;
        [ColorUsageAttribute(true, true)]
        [SerializeField] private Color activateColor;
        [SerializeField] private float explodeDurationMax;
        private Timer explodeDuration;
        private bool isExploding;

        #region Unity Lifecycle
        private void OnDrawGizmosSelected() // draw circle radius for debug
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, radius);
        }
        
        protected override void Start()
        {
            base.Start();
            stunTimer = new Timer(stunInterval);
            stunTimer.TargetTickedEvent.AddListener(TryStunAI);

            explodeDuration = new Timer(explodeDurationMax);
            explodeDuration.TargetTickedEvent.AddListener(StopExplosion);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void Update()
        {
            if(isExploding)
            {
                stunTimer.Tick(Time.deltaTime);
                explodeDuration.Tick(Time.deltaTime);
            }     
        }

        private void OnDisable()
        {
            isSet = false;

            if(meshRenderer)
            {
                meshRenderer.material.SetColor("_EmissionColor", Color.yellow);
            }

            explodeEffect.SetActive(false);
        }
        #endregion

        #region Trap Trigger Logic
        public void SubscribeModeEvent()
        {
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOn;
        }

        public void UnSubcribeModeEvent()
        {
            selfDeactivation.selfDeactivated -= ModeOn;
        }

        //! Trigger locally.
        public override bool TriggerBehavior()
        {
            if (!isSet)
                return false;

            if (!IsLocal)
                return false;

            //! Explode locally, check for AI
            isExploding = true;
            triggerSound.PlayOneShot(transform);
            explodeEffect.SetActive(true);

            TryStunAI();
            /*Collider[] creatureCollider = new Collider[1];
            int r = Physics.OverlapSphereNonAlloc(transform.position, radius, creatureCollider, UsableBlackboard.AIHitboxLayerMask);

            if (creatureCollider[0])
            {
                //Debug.LogWarning("Creature hit: " + creatureCollider[0].gameObject.name);
                if (creatureCollider[0].gameObject.GetComponentInChildren<IStunnable>() != null) 
                    creatureCollider[0].gameObject.GetComponentInChildren<IStunnable>().TryStun(stunTime);
            }*/

            //! Send event to explode.
            Vector3 activatedSpot = gameObject.transform.position;
            object[] content = { projectileID, activatedSpot };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ACTIVATED, content);

            return true;   
        }

        private void TryStunAI()
        {
            Collider[] creatureCollider = new Collider[1];
            int r = Physics.OverlapSphereNonAlloc(transform.position, radius, creatureCollider, UsableBlackboard.AIHitboxLayerMask);

            if (creatureCollider[0])
            {
                //Debug.LogWarning("Creature hit: " + creatureCollider[0].gameObject.name);
                if (creatureCollider[0].gameObject.GetComponentInChildren<IStunnable>() != null)
                    creatureCollider[0].gameObject.GetComponentInChildren<IStunnable>().TryStun(stunTime);
            }

            stunTimer.Reset();
        }

        //! Trigger network clones.
        public override void ReTriggerBehavior(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;

            if ((int)data[0] == projectileID)
            {
                if (gameObject.activeSelf)
                {
                    gameObject.transform.position = (Vector3)data[1];
                    isExploding = true;
                    triggerSound.PlayOneShot(transform);
                    explodeEffect.SetActive(true);
                }
            }            
            return;
        }

        private void StopExplosion()
        {
            isExploding = false;
            explodeDuration.Reset();
            stunTimer.Reset();
            UnSubcribeModeEvent();
            PPhysics.OnPhysicsFinished();
        }
        #endregion

        private void ModeOn()
        {
            meshRenderer.material.SetColor("_EmissionColor", activateColor);

            if(IsLocal)
                readyAudio.PlayOneShot2D();

            isSet = true;
        }
    }
}

