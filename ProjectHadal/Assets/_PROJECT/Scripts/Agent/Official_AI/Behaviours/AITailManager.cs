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

        public void Send_ApplyKnockback(in List<PlayerController> players)
        {
            if (players.IsNullOrEmpty())
                return;

            OrderReachDistancesList();
            float compareDistance = reachDistances.Last();
            Vector3 brainPosition = brain.transform.position;
            for (int i = 0; i < players.Count; i++)
            {
                float distToPlayer = Vector3.Distance(players[i].GetTarget.position, brainPosition);
                Vector3 dir = (players[i].GetTarget.position - brain.transform.position).normalized;
                bool canReachPlayer = distToPlayer <= compareDistance;
                bool inFrontOfAI = Vector3.Angle(LightOfSight, dir).IsLessOrEqualTo(ViewAngleSpan);
                if (!canReachPlayer || !inFrontOfAI) continue;

                Vector3 force = dir * whipKnockbackAmount * GetMultiplierFromFalloff(distToPlayer);
                players[i].GetComponentInChildren<IKnockable>()?.TryToKnock(force, knockDuration);
            }

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

        private float GetMultiplierFromFalloff(float distance)
        {
            float multiplier = 1f;
            if (falloffMode == KnockbackFalloffMode.Linear)
            {
                multiplier = distance.NormaliseValue(reachDistances.First(), reachDistances.Last()).Clamp0();
            }
            else if (falloffMode == KnockbackFalloffMode.Exponential)
            {
                float x = distance.NormaliseValue(reachDistances.First(), reachDistances.Last()).Clamp0();
                float neg2k = -(2f * exponentConst.Round());
                multiplier = (x - 2f).Pow(neg2k) - 2f.Pow(neg2k) * (1f - x);
            }
            multiplier += 1f;
            $"{multiplier.ToString()}".Msg();
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