using Hadal.Player;
using UnityEngine;

namespace Hadal.AI
{
    public class AITailManager : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField] private Collider whipStanceCollider;
        [SerializeField] private float whipKnockbackAmount;
        [SerializeField] private float knockDuration;
        AIBrain brain;
        AIDamageManager damageManager;

        public UpdateMode LeviathanUpdateMode => UpdateMode.MainUpdate;

        public void EnableWhipStance() => whipStanceCollider.enabled = true;
        public void DisableWhipStance() => whipStanceCollider.enabled = false;

        private void OnTriggerEnter(Collider other)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                Send_ApplyKnockback(controller);
                damageManager.Send_DamagePlayer(controller.transform, AIDamageType.Tail);
            }
        }

        public void Initialise(AIBrain brain)
        {
            this.brain = brain;
            damageManager = brain.DamageManager;
        }
        
        private void Send_ApplyKnockback(PlayerController player)
        {
            Vector3 force = brain.transform.forward * whipKnockbackAmount;
            var knockable = player.GetComponentInChildren<IKnockable>();
            if (knockable != null)
            {
                knockable.TryToKnock(force, knockDuration);
                //player.GetInfo.Rigidbody.AddForce(force, ForceMode.Impulse); <-- call this in the playercontroller side
            }

            //! Raise event here
        }

        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }
        public void DoLateUpdate(in float deltaTime) { }
    }
}