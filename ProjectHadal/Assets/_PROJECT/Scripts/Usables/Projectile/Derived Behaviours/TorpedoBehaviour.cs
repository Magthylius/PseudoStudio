using UnityEngine;
using Magthylius.DataFunctions;
using Hadal.Networking;
using static Hadal.ExplosivePoint;
using Hadal.AudioSystem;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoBehaviour : ProjectileBehaviour
    {
        [SerializeField] int explosionSelfDamage;
        [SerializeField] float audioDistanceRank = 50f;
        [SerializeField] AudioEventData closeExplosionAudio;
        [SerializeField] AudioEventData mediumExplosionAudio;
        [SerializeField] AudioEventData farExplosionAudio;
        private bool _didDamage;

        #region Unity Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        protected override void Start()
        {
            base.Start();
            impactVFXTime = 4f;
            impactDuration = new Timer(impactVFXTime);
            impactDuration.TargetTickedEvent.AddListener(StopImpactEffect);
            OnHit -= PlayImpactAudioAtSelfPosition;
            _didDamage = false;
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

            if (bubbleAsset != null)
                bubbleAsset.SetBool("Playing", true);
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
                _didDamage = true;
                ExplodeAndDespawn();
            }
            else if (UsableBlackboard.InAILayers(layer))
            {
                //! hits AI
                if(IsLocal)
                    collision.gameObject.GetComponentInChildren<IDamageable>().TakeDamage(Data.BaseDamage);
                
                _didDamage = true;
                ExplodeAndDespawn();
                
            }
            else if (UsableBlackboard.InCollidableLayers(layer))
            {
                //! hits collidables   
                ExplodeAndDespawn();
            }
            else if(UsableBlackboard.InUtilityLayers(layer))
            {
                ExplodeAndDespawn();
            }
           
        }

        private void OnTriggerEnter(Collider other)
        {
            if (projectileTriggered) return;

            projectileTriggered = true;

            int layer = other.gameObject.layer;

            if (UsableBlackboard.InAILayers(layer))
            {
                //! hits AI
                if (IsLocal)
                    other.gameObject.GetComponentInChildren<IDamageable>().TakeDamage(Data.BaseDamage);

                _didDamage = true;
                ExplodeAndDespawn();
            }
            else if(LayerMask.LayerToName(layer) == "Interactable")
            {
                ExplodeAndDespawn();
            }
        }

        private void ExplodeAndDespawn()
        {
            //if not local, only hide art asset
            if (!IsLocal)
            {
                projectileAsset.SetActive(false);
                _didDamage = false;
                return;
            }

            Vector3 collisionSpot = gameObject.transform.position;
            object[] content = { projectileID, collisionSpot };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
            ImpactBehaviour();
        }

        #region Protected Overried Function
        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            isVisualizing = true;
            particleEffect.SetActive(true);
            if(bubbleAsset != null)
                bubbleAsset.SetBool("Playing", false);
            projectileAsset.SetActive(false);
            PlayExplosionAudioBasedOnDistance();
            Create(CreateExplosionInfo());
            InvokeOnHit(_didDamage);
            _didDamage = false;
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
        
        private void PlayExplosionAudioBasedOnDistance()
        {
            if (OwnerObject == null)
            {
                "Owner object does not exist, therefore unable to play the explosion audio.".Warn();
                return;
            }
            
            float sqrDist = (transform.position - OwnerObject.transform.position).sqrMagnitude;
            float rank1 = audioDistanceRank.Sqr();
            float rank2 = audioDistanceRank.Sqr() * 2f;
            float rank3 = audioDistanceRank.Sqr() * 3f;

            if (sqrDist <= rank1)
				PlayAudioAt(closeExplosionAudio, transform);
			else if (sqrDist > rank1 && sqrDist <= rank2)
				PlayAudioAt(mediumExplosionAudio, transform);
			else if (sqrDist > rank3)
				PlayAudioAt(farExplosionAudio, transform);
        }

        private ExplosionSettings CreateExplosionInfo()
        {
            ExplosionSettings explodeInfo = new ExplosionSettings();
            explodeInfo.Damage = explosionSelfDamage;
            explodeInfo.Position = this.transform.position;
			explodeInfo.Force = 20f;
            explodeInfo.IsSpinPlayer = true;
            return explodeInfo;
        }

        public override void SetAimedPoint(Vector3 aimedPoint)
        {
            this.aimedPoint = aimedPoint;
        }
        #endregion
    }
}