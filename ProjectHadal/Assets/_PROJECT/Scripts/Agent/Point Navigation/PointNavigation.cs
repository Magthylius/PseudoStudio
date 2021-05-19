using System.Collections.Generic;
using System.Linq;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class PointNavigation : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebug;

        [Header("Timer")]
        [SerializeField] private float minLingerTime;
        [SerializeField] private float maxLingerTime;
        [SerializeField] private float timeoutNewPointTime;
        [SerializeField] private float obstacleCheckTime;

        [Header("Nav Settings")]
        [SerializeField] private int numberOfClosestPointsToConsider;
        [SerializeField] private List<NavPoint> navPoints;

        [Header("Internal Data")]
        [SerializeField] private Rigidbody rBody;
        [SerializeField] private float maxVelocity;
        [SerializeField] private float thrustForce;
        [SerializeField] private float avoidanceForce;
        [SerializeField] private float closeRepulsionForce;
        [SerializeField] private float axisStalemateDeviationForce;
        [SerializeField] private float obstacleDetectRadius;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float smoothLookAtSpeed;
        private float obstacleCheckTimer;
        private float timeoutTimer;
        private float lingerTimer;
        private bool hasReachedPoint;
        private NavPoint currentPoint;

        private void Awake()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            ResetLingerTimer();
            ResetTimeoutTimer();
            obstacleCheckTimer = 0f;
            hasReachedPoint = false;
            currentPoint = null;
            rBody ??= GetComponent<Rigidbody>();
            if (numberOfClosestPointsToConsider > navPoints.Count - 1) numberOfClosestPointsToConsider = navPoints.Count - 1;
        }

        private void Start() => currentPoint = GetClosestPointToSelf();

        private void Update()
        {
            TrySelectNewNavPoint();
            MoveTowardsCurrentNavPoint();
            HandleObstacleAvoidance();
        }

        private void MoveTowardsCurrentNavPoint()
        {
            if (currentPoint == null) return;
            Vector3 direction = currentPoint.GetDirectionTo(transform.position);
            Vector3 force = direction * (thrustForce * Time.deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);

            if (rBody.velocity.magnitude > maxVelocity)
                rBody.velocity = rBody.velocity.normalized * maxVelocity;

            Vector3 lookAt = rBody.velocity.normalized;
            transform.forward = Vector3.Lerp(transform.forward, lookAt, Time.deltaTime * smoothLookAtSpeed);

            float closeRadius = obstacleDetectRadius + 1f;
            if (currentPoint.GetSqrDistanceTo(transform.position) < closeRadius * closeRadius)
                hasReachedPoint = true;
        }

        private void HandleObstacleAvoidance()
        {
            obstacleCheckTimer -= Time.deltaTime;
            if (obstacleCheckTimer > 0f) return;

            ResetObstacleCheckTimer();
            List<Vector3> points = Physics.SphereCastAll(transform.position, obstacleDetectRadius, Vector3.zero)
                                    .Where(r => obstacleMask == (obstacleMask | (1 << r.collider.gameObject.layer)))
                                    .Select(r => r.point)
                                    .ToList();
            points.AddRange(navPoints.Select(n => n.GetPosition).Where(p => Vector3.Distance(p, transform.position) <= obstacleDetectRadius));

            if (points.IsEmpty()) return;
            points.ForEach(p =>
            {
                float dist = (transform.position - p).magnitude;
                float multiplier = 1f;
                if (dist < obstacleDetectRadius)
                    multiplier += ((obstacleDetectRadius - dist) / obstacleDetectRadius) * closeRepulsionForce;
                
                Vector3 force = (transform.position - p).normalized * avoidanceForce * multiplier;
                Vector3 cross = Vector3.Cross(force.normalized, rBody.velocity.normalized);
                if (cross.magnitude.Abs() <= 0.5f)
                {
                    if (enableDebug) "parralel".Msg();
                    Vector3 direction = force.normalized;
                    Vector3 relativeRight = new Vector3(direction.z, direction.y, -direction.x).normalized;
                    force += relativeRight * axisStalemateDeviationForce;
                }
                rBody.AddForce(force, ForceMode.VelocityChange);
            });
        }

        private void TrySelectNewNavPoint()
        {
            if (hasReachedPoint)
                lingerTimer -= Time.deltaTime;
            else
                timeoutTimer -= Time.deltaTime;

            if (lingerTimer <= 0f || timeoutTimer <= 0f)
            {
                ResetLingerTimer();
                ResetTimeoutTimer();
                var points = navPoints.Where(o => o != currentPoint);
                List<NavPoint> potentialPoints = points.OrderBy(n => n.GetSqrDistanceTo(currentPoint.GetPosition)).Take(numberOfClosestPointsToConsider).ToList();
                currentPoint = potentialPoints.RandomElement();
                hasReachedPoint = false;

                if (enableDebug) "Selecting new point".Msg();
            }
        }

        private NavPoint GetClosestPointToSelf() => navPoints.OrderBy(n => n.GetSqrDistanceTo(transform.position)).FirstOrDefault();
        private void ResetTimeoutTimer() => timeoutTimer = timeoutNewPointTime;
        private void ResetObstacleCheckTimer() => obstacleCheckTimer = obstacleCheckTime;
        private void ResetLingerTimer() => lingerTimer = GetNextLingerTime();
        private float GetNextLingerTime() => Random.Range(minLingerTime, maxLingerTime);

        private void OnDrawGizmos()
        {
            if (!enableDebug) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, obstacleDetectRadius);
        }
    }
}
