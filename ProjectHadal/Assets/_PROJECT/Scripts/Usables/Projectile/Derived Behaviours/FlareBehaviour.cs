using UnityEngine;
using Hadal.Networking;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class FlareBehaviour : ProjectileBehaviour
    {
        [Header("Flare General Settings")]
        [SerializeField] private bool isAttach;
        [SerializeField] private bool isPowerForm;
        [SerializeField] private Light flareLight;

        [Header("Flare Default Settings")]
        [SerializeField] private float defaultLightIntensity;
        [Header("Flare Powered Settings")]
        [SerializeField] private float poweredLightIntensit;

        [Header("Flare Physics")]
        [SerializeField] private bool enableRandomTorque = true;
        [SerializeField] private float randomTorqueMult = 5f;
        [SerializeField] private Rigidbody rb;

        [SerializeField] private string[] validLayer;
        public ImpulseMode impulseMode;
        public SelfDeactivationMode selfDeactivation;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!rb) rb = GetComponent<Rigidbody>();
            rb.useGravity = true;

            if (enableRandomTorque)
            {
                Vector3 randTorque = new Vector3(Random.value, Random.value, Random.value);
                //print(randTorque);
                rb.AddTorque(randTorque.normalized * randomTorqueMult, ForceMode.Impulse);
            }
        }

        public void OnDisable()
        {
            IsAttached = false;
            isPowerForm = false;
            Rigidbody.isKinematic = false;
            ProjectileCollider.enabled = true;
            rb.useGravity = false;
        }

        public void SubscribeModeEvent()
        {
            impulseMode = GetComponentInChildren<ImpulseMode>();
            impulseMode.ModeSwapped += ModeSwap;
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += ModeOff;
        }
        public void UnSubcribeModeEvent()
        {
            impulseMode.ModeSwapped -= ModeSwap;
            selfDeactivation.selfDeactivated -= ModeOff;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLocal)
            {
                return;
            }

            //If not attach mode, or already attached, return.
            if (!isAttach || IsAttached)
                return;

            foreach (string layerName in validLayer)
            {
                LayerMask layer = LayerMask.NameToLayer(layerName);
                if (collision.gameObject.layer == layer.value)
                {
                    //attach locally
                    transform.parent = collision.gameObject.transform;
                    Rigidbody.isKinematic = true;
                    ProjectileCollider.enabled = false;
                    IsAttached = true;

                    rb.useGravity = false;
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
                }
            }
        }

        private void ModeSwap(bool isPowered) 
        {
            this.isPowerForm = isPowered;
            
            if(flareLight != null)
            {
                if (!isPowerForm)
                    flareLight.intensity = defaultLightIntensity;
                else
                    flareLight.intensity = poweredLightIntensit;
            }
        }

        private void ModeOff() 
        {
            UnSubcribeModeEvent();
        } 
        public bool IsAttach { get => isAttach; set => isAttach = value; }
    }
}