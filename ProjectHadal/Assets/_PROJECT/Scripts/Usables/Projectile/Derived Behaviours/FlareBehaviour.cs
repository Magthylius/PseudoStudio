using UnityEngine;
using Hadal.Networking;

//Created by Jon, edited by Jin
namespace Hadal.Usables.Projectiles
{
    public class FlareBehaviour : ProjectileBehaviour
    {
        [Header("Flare settings")]
        [SerializeField] private bool enableRandomTorque = true;
        [SerializeField] private float randomTorqueMult = 5f;
        [SerializeField] private Rigidbody rb;

        [SerializeField] private string[] validLayer;
        [SerializeField] private bool isAttach;
        public ImpulseMode impulseMode;
        public SelfDeactivationMode selfDeactivation;

        protected override void OnEnable()
        {
            base.OnEnable();
            if(!rb) rb = GetComponent<Rigidbody>();
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
            Rigidbody.isKinematic = false;
            
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

        private void ModeSwap(bool isAttach) => this.isAttach = isAttach;
        private void ModeOff() 
        {
            UnSubcribeModeEvent();
            isAttach = false; 
        } 
        public bool IsAttach { get => isAttach; set => isAttach = value; }
    }
}