using UnityEngine;
using Hadal.Networking;
using Magthylius.DataFunctions;
using UnityEngine.VFX;
using Photon.Realtime;
//using Hadal.AI;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class HarpoonBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string[] validLayer;

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
            PPhysics.PhysicsFinished += Send_DecrementLeviathanSlowStacks;
            projectileTriggered = false;
        }

        private void OnDisable()
        {
            Rigidbody.isKinematic = false;
            IsAttached = false;
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLocal || IsAttached)
                return;

            projectileTriggered = true;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    //attach locally
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    IsAttached = true;

                    //send event data to attach          
                    bool attachedToMonster = false;
                    if (LayerMask.LayerToName(layer) == "MONSTER")
                    {
                        attachedToMonster = true;
                    }
                    else
                    {
                        attachedToMonster = false;
                    }
                    Vector3 collisionSpot = gameObject.transform.position;

                    object[] content = new object[] { projectileID, collisionSpot, attachedToMonster };
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ATTACH, content);
                    ImpactBehaviour();

                    Send_IncrementLeviathanSlowStacks();
                }
            }
        }

        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            particleEffect.SetActive(true);
            isVisualizing = true;
        }

        protected override void StopImpactEffect()
        {
            isVisualizing = false;
            particleEffect.SetActive(false);
        }

        private void Send_IncrementLeviathanSlowStacks()
        {
            var options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, 1, options);
        }
        private void Send_DecrementLeviathanSlowStacks()
        {
            PPhysics.PhysicsFinished -= Send_DecrementLeviathanSlowStacks;
            var options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_UPDATE_SLOW, -1, options);
        }
    }
}
