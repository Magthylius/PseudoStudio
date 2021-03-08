using UnityEngine;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class FlareBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string[] validLayer;
        [SerializeField] private bool isAttach;
        public ImpulseMode impulseMode;
        public SelfDeactivationMode selfDeactivation;

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeSwap;
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeSwap;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isAttach)
                return;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    isAttach = false;
                }
            }
        }

        public void ModeToggle()
        {
            isAttach = !isAttach;
        }

        private void ModeSwap(bool isAttach) => this.isAttach = isAttach;
        public bool IsAttach { get => isAttach; set => isAttach = value; }
    }
}