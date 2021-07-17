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
        [Header("Harpoon Powered Settings")]
        [SerializeField] float poweredAttachDuration;

        [Header("Misc")]
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
                    ISlowable slowable = collision.gameObject.GetComponentInChildren<ISlowable>();
                    if (slowable != null)
                    {
                        attachedToMonster = true;
                        slowable.AttachProjectile();
                        PPhysics.PhysicsFinished += slowable.DetachProjectile;
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
                    Rigidbody.isKinematic = true;
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



            /*foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    //attach locally
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    IsAttached = true;

                    //send event data to attach          
                    
                    if (UsableBlackboard.InAILayers(collision.gameObject.layer))
                    {
                        attachedToMonster = true;
                        Send_IncrementLeviathanSlowStacks();
                        //Debug.LogWarning("attached to monster");
                    }
                    else
                    {
                        attachedToMonster = false;
                    }
                    Vector3 collisionSpot = gameObject.transform.position;

                    object[] content = new object[] { projectileID, collisionSpot, attachedToMonster };
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ATTACH, content);
                    ImpactBehaviour();

                    
                }
            }*/
        }

        protected override void ImpactBehaviour()
        {
            if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
            {
                projPhysics.SwapModes();
            }

            Rigidbody.isKinematic = true;
            projectileAsset.SetActive(true);
            particleEffect.SetActive(true);
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
            }
            else
            {
                attachMode.endTime = poweredAttachDuration;
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
