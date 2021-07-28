using Hadal.Networking;
using Hadal.UI;
using Hadal.Utility;
using UnityEngine;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class SonicDartBehaviour : ProjectileBehaviour
    {
        [SerializeField] SonicDartTrackerBehaviour sonicTrackerUIBehavior;
        [SerializeField] private string[] validLayer;
        [SerializeField] private ProjectilePhysics projPhysics;
        [SerializeField] private AttachMode attachMode;
        [SerializeField] private SelfDeactivationMode selfDeactivation;
        bool attachedToMonster;
        private Timer pingTimer;
        [SerializeField]private float pingDuration;

        public void SubscribeModeEvent()
        {
            attachMode.SwitchedToAttachEvent += enableSonicDartUI;
            selfDeactivation.selfDeactivated += ModeOff;
        }

        public void UnsubscribeModeEvent()
        {
            attachMode.SwitchedToAttachEvent -= enableSonicDartUI;
            selfDeactivation.selfDeactivated -= ModeOff;
        }

        protected override void Start()
        {
            base.Start();
            pingTimer = this.Create_A_Timer()
                              .WithDuration(this.pingDuration)
                              .WithOnCompleteEvent(playPing)
                              .WithShouldPersist(true);
            pingTimer.Pause();
        }

        public void OnDisable()
        {
            if(pingTimer != null)
             pingTimer.Pause();

            if(sonicTrackerUIBehavior)
                disableSonicDartUI();

            Rigidbody.isKinematic = false;
            ProjectileCollider.enabled = true;
            IsAttached = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsAttached)
                return;

            if (collision.gameObject.layer == 11)
            {
                return;
            }

            int layer = collision.gameObject.layer;

            if (UsableBlackboard.InAILayers(layer))
            {
                //Debug.LogWarning("hit ai!");
                if (IsLocal)
                {
                    attachedToMonster = true;
                }
            }
            else
            {
                attachedToMonster = false;
            }

            if (!UsableBlackboard.InPlayerLayers(layer) && !UsableBlackboard.InUtilityLayers(layer))
            {
                if (IsLocal)
                {
                    transform.parent = collision.gameObject.transform;
                    IsAttached = true;

                    Vector3 collisionSpot = gameObject.transform.position;
                    object[] content = new object[] { projectileID, collisionSpot, attachedToMonster };
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ATTACH, content);
                    ImpactBehaviour();
                }
            }
        }

        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            ProjectileCollider.enabled = false;

            if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
            {
                projPhysics.SwapModes();
            }

            pingTimer.Restart();
            PlayImpactAudioAtSelfPosition(false);
        }

        private void enableSonicDartUI()
        {
            sonicTrackerUIBehavior.Activate();
        }

        private void disableSonicDartUI()
        {
            sonicTrackerUIBehavior.Deactivate();
        }

        private void playPing()
        {
            PlayTriggerAudioAtSelfPosition();
            pingTimer.RestartWithDuration(pingDuration);
        }

        private void ModeOff()
        {
            UnsubscribeModeEvent();
        }

        public void InjectUIDependency(SonicDartTrackerBehaviour uiTracker)
        {
            sonicTrackerUIBehavior = uiTracker;
        }
    }
}