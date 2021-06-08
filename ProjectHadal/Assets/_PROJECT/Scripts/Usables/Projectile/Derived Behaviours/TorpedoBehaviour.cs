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
                    /*neManager.RaiseEvent(ByteEvents.PLAYER_UTILITIES_LAUNCH, content);*/
                    for (int i=0; i < GameManager.Instance.pViewList.Count; i++)
                    {
                        if (GetShooterID() == GameManager.Instance.pViewList[i].ViewID)
                        {
                            PPhysics.OnPhysicsFinished();
                            print(projectileID + "sending event to despawn");
                            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_DESPAWN,projectileID);
                        }
                    }
                    
                    break;
                }
            }
        }
    }
}