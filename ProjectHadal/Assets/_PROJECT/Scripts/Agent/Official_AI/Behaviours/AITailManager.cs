using System.Collections.Generic;
using System.Linq;
using Hadal.Player;
using NaughtyAttributes;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class AITailManager : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField] private Collider whipStanceCollider;
        [SerializeField] private List<float> reachDistances;
        [SerializeField] private float whipKnockbackAmount;
        [SerializeField] private float knockDuration;
        [SerializeField] private KnockbackFalloffMode falloffMode;
        [SerializeField] private ExplosivePoint.ExplosionSettings knockbackSettings;
        [SerializeField, Range(0f, 4f)] private float exponentConst;

        [SerializeField] private float viewAngle;
        AIBrain brain;
        AIDamageManager damageManager;

        public UpdateMode LeviathanUpdateMode => UpdateMode.MainUpdate;

        public void EnableWhipStance()
        {
            // knockbackSettings.Position = brain.transform.position;
            // ExplosivePoint.Create(knockbackSettings);
        }
        public void DisableWhipStance()
        {
            // whipStanceCollider.enabled = false;
        }

        // private void OnTriggerEnter(Collider other)
        // {
        //     PlayerController controller = other.GetComponent<PlayerController>();
        //     if (controller != null)
        //     {
        //         Send_ApplyKnockback(controller);
        //         damageManager.Send_DamagePlayer(controller.transform, AIDamageType.Tail);
        //     }
        // }

        public void Initialise(AIBrain brain)
        {
            this.brain = brain;
            damageManager = brain.DamageManager;
        }

        private void Send_ApplyKnockback(PlayerController player)
        {
            OrderReachDistancesList();
            float distToPlayer = Vector3.Distance(player.GetTarget.position, brain.transform.position);
            Vector3 dir = (player.GetTarget.position - brain.transform.position).normalized;
            bool canReachPlayer = distToPlayer <= reachDistances.Last();
            bool inFrontOfAI = Vector3.Angle(LightOfSight, dir).IsLessOrEqualTo(ViewAngleSpan);

            if (!canReachPlayer || !inFrontOfAI) return;

            int r = GetRankFromReachDistances(distToPlayer);
            if (r == -1)
                return;

            float forceMultiplier = 1f;
            for (int rank = 1; rank <= reachDistances.Count; rank++)
            {
                float distance = reachDistances[rank];
                forceMultiplier = GetMultiplierFromFalloff(distToPlayer);
            }

            float additionalForce = forceMultiplier;
            Vector3 force = dir * whipKnockbackAmount * additionalForce;
            var knockable = player.GetComponentInChildren<IKnockable>();
            knockable?.TryToKnock(force, knockDuration);

            //! Raise event here

        }

        private void OrderReachDistancesList() => reachDistances = reachDistances.OrderBy(dist => dist).ToList();
        private int GetRankFromReachDistances(float distance)
        {
            for (int rank = 0; rank < reachDistances.Count; rank++)
            {
                if (distance <= reachDistances[rank] || rank == reachDistances.Count - 1)
                    return rank;

                float thisDist = reachDistances[rank];
                float nextDist = reachDistances[rank + 1];
                if (distance > thisDist && distance <= nextDist)
                    return rank + 1;
            }
            return -1;
        }
        [Button("Test")]
        private float GetMultiplierFromFalloff(float distance = 1f)
        {
            float multiplier = 1f;
            if (falloffMode == KnockbackFalloffMode.Linear)
            {
                multiplier = distance.NormaliseValue(reachDistances.First(), reachDistances.Last()).Clamp01();
                multiplier += 1f;
            }
            else if (falloffMode == KnockbackFalloffMode.Exponential)
            {
                float x = distance.NormaliseValue(reachDistances.First(), reachDistances.Last()).Clamp01();
                float neg2k = -(2 * exponentConst);
                multiplier = (x - 2).Pow(neg2k) - 2.Pow(neg2k) * (1 - x);
            }
            multiplier.Msg();
            return multiplier;
        }

        private float ViewAngleSpan => viewAngle / 2f;
        private Vector3 LightOfSight => brain.transform.forward;

        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }
        public void DoLateUpdate(in float deltaTime) { }

        public enum KnockbackFalloffMode
        {
            Linear,
            Exponential
        }
    }
}