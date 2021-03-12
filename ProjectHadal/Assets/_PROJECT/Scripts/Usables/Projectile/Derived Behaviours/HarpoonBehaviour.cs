using UnityEngine;
using Hadal.AI;

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
            if (IsAttached)
                return;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    IsAttached = true;

                    //if its AI.
                    if (collision.gameObject.GetComponent<AIBrain>())
                    {
                        //  collision.gameObject.GetComponent<AIBrain>().SetIsStunned(true);
                    }
                }
            }
        }
    }
}
