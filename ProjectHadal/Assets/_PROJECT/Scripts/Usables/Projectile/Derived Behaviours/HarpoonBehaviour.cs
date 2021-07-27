using UnityEngine;
using Hadal.Networking;
using Magthylius.DataFunctions;
using UnityEngine.VFX;
using Photon.Realtime;
using Tenshi.UnitySoku;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class HarpoonBehaviour : ProjectileBehaviour
    {
        [Header("Harpoon General Settings")]
        [SerializeField] private bool isPowerForm;
        [SerializeField] private string[] validLayer;
        bool attachedToMonster = false;

        [Header("Harpoon Default Settings")]
        [SerializeField] float defaultAttachDuration;
        [SerializeField] int defaultSlowCount;
        [Header("Harpoon Powered Settings")]
        [SerializeField] float poweredAttachDuration;
        [SerializeField] int poweredSlowCount;

        [Header("Misc")]
        private ISlowable slowable;
        public ProjectilePhysics projPhysics;
        public ImpulseMode impulseMode;
        public AttachMode attachMode;
        public SelfDeactivationMode selfDeactivation;

        #region Unity Lifecycle
        protected override void Start()
        {
            base.Start();
            impactVFXTime = particleEffect.GetComponent<VisualEffect>().GetFloat("Dust Lifetime");
            impactDuration = new Timer(5f);
            impactDuration.TargetTickedEvent.AddListener(StopImpactEffect);
        }
        private void Update()
        {
            if (isVisualizing)
            {
                impactDuration.Tick(Time.deltaTime);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //PPhysics.PhysicsFinished += Send_DecrementLeviathanSlowStacks;
            projectileTriggered = false;
            attachedToMonster = false;
        }

        private void OnDisable()
        {
            Rigidbody.isKinematic = false;
            ProjectileCollider.enabled = true;
            IsAttached = false;
        }
        #endregion

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeSwap;
            attachMode = GetComponentInChildren<AttachMode>();
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOff;
        }
        public void UnSubcribeModeEvent()
        {
            impulseMode.ModeSwapped -= ModeSwap;
            selfDeactivation.selfDeactivated -= ModeOff;
        }

        public void UnSubscribeDetachProjectile()
        {
            if (slowable != null)
            {
                PPhysics.PhysicsFinished -= UnSubscribeDetachProjectile;

                int slowCount = 0;

                if (isPowerForm)
                    slowCount = poweredSlowCount;
                else
                    slowCount = defaultSlowCount;

                for (int i = 0; i < slowCount; i++)
                {
                    PPhysics.PhysicsFinished -= slowable.DetachProjectile;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsAttached)
                return;

            // If it's AI layer, return;
            if (collision.gameObject.layer == 11)
            {
                return;
            }

            projectileTriggered = true;

            int layer = collision.gameObject.layer;
            if (UsableBlackboard.InAILayers(layer))
            {
                //Debug.LogWarning("hit ai!");
                if (IsLocal)
                {
                    slowable = collision.gameObject.GetComponentInChildren<ISlowable>();
                    if (slowable != null)
                    {
                        attachedToMonster = true;
                        int slowCount = 0;
                        
                        if (isPowerForm)
                            slowCount = poweredSlowCount;
                        else
                            slowCount = defaultSlowCount;

                        for(int i = 0; i < slowCount; i++)
                        {
                            slowable.AttachProjectile();
                            PPhysics.PhysicsFinished += slowable.DetachProjectile;
                            PPhysics.PhysicsFinished += UnSubscribeDetachProjectile;
                        }    
                    }
                    else { $"AI was hit but it does not have an ISlowable interface implemented. Is the collider on the wrong layer ({collision.gameObject.name})?".Msg(); }
                }
                else //if non local, hide art asset upon impact.
                {
                    projectileAsset.SetActive(false);
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
                else //if non local, hide art asset upon impact.
                {
                    projectileAsset.SetActive(false);
                }

            }
        }

        protected override void ImpactBehaviour()
        {
            if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
            {
                projPhysics.SwapModes();
            }   

            Rigidbody.isKinematic = true;
            ProjectileCollider.enabled = false;
            projectileAsset.SetActive(true);
            particleEffect.SetActive(true);
            PlayImpactAudioAtSelfPosition(false);
            isVisualizing = true;
        }

        protected override void StopImpactEffect()
        {
            isVisualizing = false;
            particleEffect.SetActive(false);
        }

        private void ModeSwap(bool isPowered)
        {
            this.isPowerForm = isPowered;

            if (!isPowerForm)
            {
                attachMode.endTime = defaultAttachDuration;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                attachMode.endTime = poweredAttachDuration;
                transform.localScale = new Vector3(2, 2, 2);
            }
        }

        private void ModeOff()
        {
            UnSubcribeModeEvent();
        }
        /*private void Send_IncrementLeviathanSlowStacks()
        {
            var options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, 1);
        }
        private void Send_DecrementLeviathanSlowStacks()
        {
            PPhysics.PhysicsFinished -= Send_DecrementLeviathanSlowStacks;
            
            if (!attachedToMonster) return;
            //var options = new RaiseEventOptions() { Receivers = ReceiverGroup. };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, -1);
        }*/
    }
}
