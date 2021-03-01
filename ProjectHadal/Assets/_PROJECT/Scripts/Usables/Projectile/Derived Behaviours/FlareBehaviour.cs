using UnityEngine;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class FlareBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string wallLayer = string.Empty;
        [SerializeField] private bool IsAttach;
        public ImpulseMode impulseMode;

        private void OnCollisionEnter(Collision collision)
        {
            LayerMask layer = LayerMask.NameToLayer(wallLayer);
            if (collision.gameObject.layer == layer.value)
            {
                if(IsAttach == true)
                {
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                }                 
            }
        }

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeToggle;
        }

        public void ModeToggle()
        {
            IsAttach = !IsAttach;
        }

        private void ModeToggle(bool IsAttach) => this.IsAttach = IsAttach;
    }
}