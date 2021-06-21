using UnityEngine;
using Magthylius.DataFunctions;
using Hadal.Networking;
using static Hadal.ExplosivePoint;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoBehaviour : ProjectileBehaviour
    {
        [SerializeField] private float impactVFXTime = 5f;
        private bool projectileTriggered = false;
        
        #region Unity Lifecycle

        protected override void OnEnable()
        {
            projectileTriggered = false;
        }
        
        protected override void Start()
        {
            base.Start();
            impactDuration = new Timer(impactVFXTime);
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
            Rigidbody.isKinematic = false;
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if(!IsLocal || projectileTriggered) return;

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
                Vector3 collisionSpot = gameObject.transform.position;
                object[] content = {projectileID, collisionSpot};
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
                ImpactBehaviour();
            }
        }

        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            isVisualizing = true;
            particleEffect.SetActive(true);
            projectileAsset.SetActive(false);
            Create(CreateExplosionInfo());
        }

        protected override void StopImpactEffect()
        {
            isVisualizing = false;
            particleEffect.SetActive(false);
            projectileAsset.SetActive(true);
            PPhysics.OnPhysicsFinished();
        }

        private ExplosionSettings CreateExplosionInfo()
        {
            ExplosionSettings explodeInfo = new ExplosionSettings();
            explodeInfo.Position = this.transform.position;
            return explodeInfo;
        }
    }
}