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

        public void OnDisable()
        {
            IsAttached = false;
        }

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeSwap;
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOff;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isAttach || IsAttached)
                return;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    IsAttached = true;
                }
            }
        }

        private void ModeSwap(bool isAttach) => this.isAttach = isAttach;
        private void ModeOff() => isAttach = false;
        public bool IsAttach { get => isAttach; set => isAttach = value; }
    }
}