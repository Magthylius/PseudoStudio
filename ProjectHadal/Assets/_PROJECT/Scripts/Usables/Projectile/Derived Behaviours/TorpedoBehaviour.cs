using UnityEngine;
using Magthylius.DataFunctions;
using Hadal;
using Hadal.Networking;
//using Hadal.AI;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoBehaviour : ProjectileBehaviour
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
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if(!IsLocal)
            { 
                return;
            }

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    
                    if (LayerMask.LayerToName(layer) == "MONSTER")
                    {
                        collision.gameObject.GetComponentInChildren<IDamageable>().TakeDamage(Data.BaseDamage);
                    }

                    Vector3 collisionSpot = gameObject.transform.position;
                    object[] content = new object[] {projectileID, collisionSpot};
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
                    ImpactBehaviour();
                    return;
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