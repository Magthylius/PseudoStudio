using Hadal.Player;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class AITailManager : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField] private Collider whipStanceCollider;
        [SerializeField] private float whipKnockbackAmount;
        [SerializeField] private float knockDuration;

        [SerializeField] private float reachDistance;
        [SerializeField] private float viewAngle;
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

            AISenseDetection sense = brain.SenseDetection;
            if (reachDistance > sense.MajorDetectionRadius)
                reachDistance = reachDistance.Clamp(1f, sense.MajorDetectionRadius);
        }
        
        private void Send_ApplyKnockback(PlayerController player)
        {
            float dist = Vector3.Distance(player.GetTarget.position, brain.transform.position);
            Vector3 dir = (player.GetTarget.position - brain.transform.position).normalized;
            bool canReachPlayer = dist <= reachDistance;
            bool inFrontOfAI = Vector3.Angle(LightOfSight, dir).IsLessOrEqualTo(ViewAngleSpan);

            if (!canReachPlayer || !inFrontOfAI) return;
            
            float additionalForce = reachDistance - dist;
            Vector3 force = dir * whipKnockbackAmount * additionalForce;
            var knockable = player.GetComponentInChildren<IKnockable>();
            knockable?.TryToKnock(force, knockDuration);

            //! Raise event here

        }

        private float ViewAngleSpan => viewAngle / 2f;
        private Vector3 LightOfSight => brain.transform.forward;

        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }
        public void DoLateUpdate(in float deltaTime) { }
    }
}