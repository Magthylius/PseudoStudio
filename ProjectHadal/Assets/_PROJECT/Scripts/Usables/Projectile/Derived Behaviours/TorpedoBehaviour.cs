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

        [Header("Explode Effect")]
        [SerializeField] private GameObject particleEffect;
        private Timer explodeDuration;
        private bool isExploding;

        #region Unity Lifecycle
        protected override void Start()
        {
            base.Start();
            explodeDuration = new Timer(2f);
            explodeDuration.TargetTickedEvent.AddListener(StopExplosion);
        }
        private void Update()
        {
            if (isExploding)
            {
                explodeDuration.Tick(Time.deltaTime);
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
                print("valid layer found");
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

        public override void ImpactBehaviour()
        {
            print("called");
            particleEffect.SetActive(true);
            isExploding = true;
        }

        private void StopExplosion()
        {
            isExploding = false;
            PPhysics.OnPhysicsFinished();
        }
    }
}