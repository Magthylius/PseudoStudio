using UnityEngine;
using Hadal;
using Hadal.Networking;
//using Hadal.AI;

namespace Hadal.Usables.Projectiles
{
    public class TorpedoBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string[] validLayer;

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

                    PPhysics.OnPhysicsFinished();
                    Vector3 collisionSpot = gameObject.transform.position;
                    object[] content = new object[] {projectileID, collisionSpot};
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
                    return;
                }
            }
        }
    }
}