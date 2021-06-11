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
            //Check if projectile is local.
         /*   for (int i = 0; i < GameManager.Instance.pViewList.Count; i++)
            {
                if (GetShooterID() != GameManager.Instance.pViewList[i].ViewID || !GameManager.Instance.pViewList[i].IsMine)
                {
                    return;
                }
            }*/
            if(!isLocal)
            {
                return;
            }

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    //if its AI.
                    /*if (collision.gameObject.GetComponent<AIBrain>())
                    {
                        collision.gameObject.GetComponent<AIBrain>().HealthManager.TakeDamage(Data.BaseDamage);
                    }*/
                    PPhysics.OnPhysicsFinished();
                    Vector3 collisionSpot = gameObject.transform.position;
                    print(projectileID + "sending event to despawn");
                    object[] content = new object[] {projectileID, collisionSpot};
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN, content);
                    return;
                }
            }
        }
    }
}