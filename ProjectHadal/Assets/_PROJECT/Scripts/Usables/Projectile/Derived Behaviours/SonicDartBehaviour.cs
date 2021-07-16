using Hadal.UI;
using UnityEngine;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class SonicDartBehaviour : ProjectileBehaviour
    {
        [SerializeField] private string[] validLayer;
        [SerializeField] private ProjectilePhysics projPhysics;
        [SerializeField] private AttachMode attachMode;
        [SerializeField] private SelfDeactivationMode selfDeactivation;
        public void SubscribeModeEvent()
        {
            attachMode.SwitchedToAttachEvent += enableSonicDartUI;
            selfDeactivation.selfDeactivated += ModeOff;
        }

        public void UnsubscribeModeEvent()
        {
            attachMode.SwitchedToAttachEvent -= enableSonicDartUI;
            selfDeactivation.selfDeactivated -= ModeOff;
        }

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

                    if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
                    {
                        projPhysics.SwapModes();
                    }
                }
            }
        }

        private void enableSonicDartUI()
        {
            //enable UI here.
        }

        private void ModeOff()
        {
            UnsubscribeModeEvent();
        }
    }
}