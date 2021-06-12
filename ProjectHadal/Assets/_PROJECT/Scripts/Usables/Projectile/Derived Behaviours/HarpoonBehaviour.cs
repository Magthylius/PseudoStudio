using UnityEngine;
using Hadal.Networking;
//using Hadal.AI;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class HarpoonBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string[] validLayer;

        public void OnDisable()
        {
            IsAttached = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLocal)
            {
                return;
            }

            if (IsAttached || IsAttached)
                return;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    //attach locally
                    print(projectileID + "harpoon attach locally");
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
                    //if its AI.
                    //if (collision.gameObject.GetComponent<AIBrain>())
                    // {
                    //  collision.gameObject.GetComponent<AIBrain>().SetIsStunned(true);
                    // }
                }
            }
        }
    }
}
