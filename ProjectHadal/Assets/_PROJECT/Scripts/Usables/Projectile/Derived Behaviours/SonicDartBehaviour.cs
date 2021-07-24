using Hadal.Networking;
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
        bool attachedToMonster;
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
            Rigidbody.isKinematic = false;
            ProjectileCollider.enabled = true;
            IsAttached = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsAttached)
                return;

            if (collision.gameObject.layer == 11)
            {
                return;
            }

            int layer = collision.gameObject.layer;

            if (UsableBlackboard.InAILayers(layer))
            {
                //Debug.LogWarning("hit ai!");
                if (IsLocal)
                {
                    attachedToMonster = true;
                }
            }
            else
            {
                attachedToMonster = false;
            }

            if (!UsableBlackboard.InPlayerLayers(layer) && !UsableBlackboard.InUtilityLayers(layer))
            {
                if (IsLocal)
                {
                    transform.parent = collision.gameObject.transform;
                    IsAttached = true;

                    Vector3 collisionSpot = gameObject.transform.position;
                    object[] content = new object[] { projectileID, collisionSpot, attachedToMonster };
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PROJECTILE_ATTACH, content);
                    ImpactBehaviour();
                }
            }

           /* foreach (string layerName in validLayer)
            {
                LayerMask layers = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layers.value)
                {
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    IsAttached = true;

                    if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
                    {
                        projPhysics.SwapModes();
                    }
                }
            }*/
        }

        protected override void ImpactBehaviour()
        {
            Rigidbody.isKinematic = true;
            ProjectileCollider.enabled = false;

            if (projPhysics.GetCurrentMode() == ProjectileMode.ProjectileModeEnum.IMPULSE)
            {
                projPhysics.SwapModes();
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