using UnityEngine;
using Magthylius.DataFunctions;
using Hadal.Networking;
using static Hadal.ExplosivePoint;
using Hadal.AudioSystem;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoBehaviour : ProjectileBehaviour
    {
        private Vector3 aimedPoint = Vector3.zero;
        
        #region Unity Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            transform.LookAt(aimedPoint);
        }
        
        protected override void Start()
        {
            base.Start();
            //impactVFXTime = particleEffect.GetComponent<VisualEffect>().GetFloat("Explosion Lifetime");
            impactVFXTime = 4f;
            impactDuration = new Timer(impactVFXTime);
            impactDuration.TargetTickedEvent.AddListener(StopImpactEffect);
        }
        
        private void Update()
        {
            if (isVisualizing)
            {
                impactDuration.Tick(Time.deltaTime);
            }
            /*transform.LookAt(aimedPoint);*/
        }

        private void OnDisable()
        {
            Rigidbody.isKinematic = false;
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if (projectileTriggered) return;

            projectileTriggered = true;

            int layer = collision.gameObject.layer;
            if (UsableBlackboard.InPlayerLayers(layer))
            {
                //! hits player
                
                ExplodeAndDespawn();
            }
            else if (UsableBlackboard.InAILayers(layer))
            {
                //! hits AI
                if(IsLocal)
                    collision.gameObject.GetComponentInChildren<IDamageable>().TakeDamage(Data.BaseDamage);
                
                ExplodeAndDespawn();
                
            }
            else if (UsableBlackboard.InCollidableLayers(layer))
            {
                //! hits collidables   
                ExplodeAndDespawn();
            }
            

            void ExplodeAndDespawn()
            {
                //if not local, only hide art asset
                if(!IsLocal)
                {
                    projectileAsset.SetActive(false);
                    return;
                }

                Vector3 collisionSpot = gameObject.transform.position;
                object[] content = {projectileID, collisionSpot};
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
                ImpactBehaviour();
            }
        }

        #region Protected Overried Function
        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            isVisualizing = true;
            particleEffect.SetActive(true);
            projectileAsset.SetActive(false);
            Create(CreateExplosionInfo());
            InvokeOnHit(false);
        }

        protected override void StopImpactEffect()
        {
            isVisualizing = false;
            particleEffect.SetActive(false);
            projectileAsset.SetActive(true);
            PPhysics.OnPhysicsFinished();
        }
        #endregion

        #region Misc Function
        
        private ExplosionSettings CreateExplosionInfo()
        {
            ExplosionSettings explodeInfo = new ExplosionSettings();
            explodeInfo.Position = this.transform.position;
            return explodeInfo;
        }

        public void SetAimedPoint(Vector3 aimedPoint)
        {
            this.aimedPoint = aimedPoint;
        }
        #endregion
    }
}