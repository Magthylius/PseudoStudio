using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class PointNavigation : MonoBehaviour, IUnityServicer
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
        [SerializeField] private Transform pilotTrans;
        [SerializeField] private Rigidbody rBody;

        [Header("Internal Data")]
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
        private bool canAutoSelectNavPoints;
        private bool isOnCustomPath;
        private bool canPath;

        private void Awake() => Initialise();
        private void Update() => DoUpdate(DeltaTime);
        private void FixedUpdate() => DoFixedUpdate(FixedDeltaTime);


        #region Public Methods

        public void Initialise()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            ResetLingerTimer();
            ResetTimeoutTimer();
            obstacleCheckTimer = 0f;
            hasReachedPoint = false;
            currentPoint = null;
            canAutoSelectNavPoints = true;
            isOnCustomPath = false;
            canPath = true;
            rBody ??= GetComponent<Rigidbody>();
            if (numberOfClosestPointsToConsider > navPoints.Count - 1) numberOfClosestPointsToConsider = navPoints.Count - 1;
            currentPoint = GetClosestPointToSelf();
        }
        public void DoUpdate(in float deltaTime)
        {
            if (pilotTrans == null) return;
            if (canPath)
            {
                TrySelectNewNavPoint(deltaTime);
                MoveTowardsCurrentNavPoint(deltaTime);
            }
            HandleObstacleAvoidance(deltaTime);
        }
        public void DoFixedUpdate(in float fixedDeltaTime)
        {

        }

        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;

        public void SetCanPath(bool statement)
        {
            canPath = statement;

            if (canPath)
            {
                ResetLingerTimer();
                ResetTimeoutTimer();
                obstacleCheckTimer = 0f;
                return;
            }
            rBody.velocity = Vector3.zero;
        }

        public void SetCustomPath(NavPoint target, bool targetIsPlayer)
        {
            if (target == null) return;
            isOnCustomPath = true;
            currentPoint = target;
            canAutoSelectNavPoints = !targetIsPlayer;
        }

        public void StopCustomPath(bool instantlyFindNewNavPoint = false)
        {
            canAutoSelectNavPoints = true;
			if (isOnCustomPath)
			{
				isOnCustomPath = false;
				StartCoroutine(DestroyAndRegenerateCurrentNavPoint(instantlyFindNewNavPoint));
			}

            IEnumerator DestroyAndRegenerateCurrentNavPoint(bool justFindNewPoint)
            {
                Destroy(currentPoint);
                yield return null;
                
                if (justFindNewPoint)
                {
                    ResetLingerTimer();
                    ResetTimeoutTimer();
                    SelectNewNavPoint();
                    yield break;
                }
                currentPoint = GetClosestPointToSelf();
            }
        }

        #endregion

        #region Private Methods

        private void MoveTowardsCurrentNavPoint(in float deltaTime)
        {
            if (currentPoint == null) return;
            Vector3 direction = currentPoint.GetDirectionTo(pilotTrans.position);
            Vector3 force = direction * (thrustForce * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);

            if (rBody.velocity.magnitude > maxVelocity)
                rBody.velocity = rBody.velocity.normalized * maxVelocity;

            Vector3 lookAt = rBody.velocity.normalized;
            pilotTrans.forward = Vector3.Lerp(pilotTrans.forward, lookAt, deltaTime * smoothLookAtSpeed);

            float closeRadius = obstacleDetectRadius + 1f;
            if (currentPoint.GetSqrDistanceTo(pilotTrans.position) < closeRadius * closeRadius)
            {
                hasReachedPoint = true;
                StopCustomPath();
            }
        }

        private void HandleObstacleAvoidance(in float deltaTime)
        {
            obstacleCheckTimer -= deltaTime;
            if (obstacleCheckTimer > 0f) return;

            ResetObstacleCheckTimer();
            List<Vector3> points = Physics.SphereCastAll(pilotTrans.position, obstacleDetectRadius, Vector3.zero)
                                    .Where(r => obstacleMask == (obstacleMask | (1 << r.collider.gameObject.layer)))
                                    .Select(r => r.point)
                                    .ToList();
            points.AddRange(navPoints.Select(n => n.GetPosition).Where(p => Vector3.Distance(p, pilotTrans.position) <= obstacleDetectRadius));

            if (points.IsEmpty()) return;
            points.ForEach(p =>
            {
                float dist = (pilotTrans.position - p).magnitude;
                float multiplier = 1f;
                if (dist < obstacleDetectRadius)
                    multiplier += ((obstacleDetectRadius - dist) / obstacleDetectRadius) * closeRepulsionForce;

                Vector3 force = (pilotTrans.position - p).normalized * avoidanceForce * multiplier;
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

        private void TrySelectNewNavPoint(in float deltaTime)
        {
            if (isOnCustomPath || !canAutoSelectNavPoints) return;

            if (hasReachedPoint)
                lingerTimer -= deltaTime;
            else
                timeoutTimer -= deltaTime;

            if (lingerTimer <= 0f || timeoutTimer <= 0f)
            {
                ResetLingerTimer();
                ResetTimeoutTimer();
                SelectNewNavPoint();
            }
        }

        private void SelectNewNavPoint()
        {
            var points = navPoints.Where(o => o != currentPoint);
            List<NavPoint> potentialPoints = points.OrderBy(n => n.GetSqrDistanceTo(currentPoint.GetPosition)).Take(numberOfClosestPointsToConsider).ToList();
            currentPoint = potentialPoints.RandomElement();
            hasReachedPoint = false;

            if (enableDebug) "Selecting new point".Msg();
        }

        private NavPoint GetClosestPointToSelf() => navPoints.OrderBy(n => n.GetSqrDistanceTo(pilotTrans.position)).FirstOrDefault();
        private void ResetTimeoutTimer() => timeoutTimer = timeoutNewPointTime;
        private void ResetObstacleCheckTimer() => obstacleCheckTimer = obstacleCheckTime;
        private void ResetLingerTimer() => lingerTimer = GetNextLingerTime();
        private float GetNextLingerTime() => Random.Range(minLingerTime, maxLingerTime);

        #endregion

        private void OnDrawGizmos()
        {
            if (!enableDebug || pilotTrans == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pilotTrans.position, obstacleDetectRadius);
        }
    }
}
