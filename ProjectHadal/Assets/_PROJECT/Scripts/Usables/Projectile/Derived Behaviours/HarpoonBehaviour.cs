using UnityEngine;
using Hadal.Networking;
using Magthylius.DataFunctions;
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
            impactDuration = new Timer(2f);
            impactDuration.TargetTickedEvent.AddListener(StopImpactEffect);
        }
        private void Update()
        {
            if (isVisualizing)
            {
                impactDuration.Tick(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            particleEffect.SetActive(false);
            IsAttached = false;
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLocal)
            {
                return;
            }

            if (IsAttached)
                return;

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
                    //if its AI.
                    //if (collision.gameObject.GetComponent<AIBrain>())
                    // {
                    //  collision.gameObject.GetComponent<AIBrain>().SetIsStunned(true);
                    // }
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
            Rigidbody.isKinematic = false;
            PPhysics.OnPhysicsFinished();
        }
    }
}
