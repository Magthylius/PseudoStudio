using UnityEngine;
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
                    PPhysics.OnPhysicsFinished();
                    break;
                }
            }
        }
    }
}