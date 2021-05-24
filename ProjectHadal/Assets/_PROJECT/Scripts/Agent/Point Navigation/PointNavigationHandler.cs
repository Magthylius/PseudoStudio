using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class PointNavigationHandler : MonoBehaviour, IUnityServicer
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebug;

        [Header("Timer Settings")]
        [SerializeField] private float minLingerTime;
        [SerializeField] private float maxLingerTime;
        [SerializeField] private float timeoutNewPointTime;
        [SerializeField] private float obstacleCheckTime;
        private float obstacleCheckTimer;
        private float timeoutTimer;
        private float lingerTimer;
		
		[Header("Force Settings")]
		[SerializeField] private float maxVelocity;
        [SerializeField] private float thrustForce;
		[SerializeField] private float attractionForce;
        [SerializeField] private float avoidanceForce;
        [SerializeField] private float closeRepulsionForce;
        [SerializeField] private float axisStalemateDeviationForce;
        [SerializeField] private float obstacleDetectRadius;
        [SerializeField] private float smoothLookAtSpeed;
		[SerializeField] private LayerMask obstacleMask;

        [Header("Nav Components")]
        [SerializeField] private int numberOfClosestPointsToConsider;
        [SerializeField] private Transform pilotTrans;
        [SerializeField] private Rigidbody rBody;

        [Header("Internal Data")]
		[SerializeField, ReadOnly] private List<NavPoint> navPoints;
		[SerializeField, ReadOnly] private List<Vector3> repulsionPoints;
        [SerializeField, ReadOnly] private bool hasReachedPoint;
        [SerializeField, ReadOnly] private bool canAutoSelectNavPoints;
        [SerializeField, ReadOnly] private bool isOnCustomPath;
        [SerializeField, ReadOnly] private bool canPath;
        [SerializeField, ReadOnly] private NavPoint currentPoint;
        
        private void Awake() => Initialise();
        private void Update() => DoUpdate(DeltaTime);
        private void FixedUpdate() => DoFixedUpdate(FixedDeltaTime);


        #region Public Methods

        public void Initialise()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            navPoints.ForEach(p => p.Initialise());
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
            repulsionPoints = new List<Vector3>();
        }
        public void DoUpdate(in float deltaTime)
        {
            
        }
        public void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (pilotTrans == null) return;
            if (canPath)
            {
                TrySelectNewNavPoint(fixedDeltaTime);
				MoveForwards(fixedDeltaTime);
                MoveTowardsCurrentNavPoint(fixedDeltaTime);
            }
            HandleObstacleAvoidance(fixedDeltaTime);
        }

        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
        public float ObstacleDetectionRadius => obstacleDetectRadius;
        public Transform PilotTransform => pilotTrans;

        public void AddRepulsionPoint(Vector3 point)
        {
            if (!repulsionPoints.Contains(point))
                repulsionPoints.Add(point);
        }

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
            ResetLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug) "Setting custom path".Msg();
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
				currentPoint.Deselect();
                Destroy(currentPoint.gameObject);
                if (enableDebug) "Stopping custom path".Msg();
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

		private void MoveForwards(in float deltaTime)
		{
			Vector3 force = pilotTrans.forward * (thrustForce * deltaTime);
			rBody.AddForce(force, ForceMode.VelocityChange);
		}

        private void MoveTowardsCurrentNavPoint(in float deltaTime)
        {
            if (currentPoint == null) return;
            Vector3 direction = currentPoint.GetDirectionTo(pilotTrans.position);
            Vector3 force = direction * (attractionForce * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);

            if (rBody.velocity.magnitude > maxVelocity)
                rBody.velocity = rBody.velocity.normalized * maxVelocity;

            Vector3 lookAt = rBody.velocity.normalized;
            pilotTrans.forward = Vector3.Lerp(pilotTrans.forward, lookAt, deltaTime * smoothLookAtSpeed);

            float closeRadius = obstacleDetectRadius - 2f;
            if (!hasReachedPoint && currentPoint.GetSqrDistanceTo(pilotTrans.position) < closeRadius * closeRadius)
            {
                hasReachedPoint = true;
                if (enableDebug) "Point Reached".Msg();
            }
        }

        private void HandleObstacleAvoidance(in float deltaTime)
        {
            obstacleCheckTimer -= deltaTime;
            if (obstacleCheckTimer > 0f) return;

            ResetObstacleCheckTimer();
            // List<Vector3> points = Physics.SphereCastAll(pilotTrans.position, obstacleDetectRadius, Vector3.zero)
            //                         .Where(r => obstacleMask == (obstacleMask | (1 << r.collider.gameObject.layer)))
            //                         .Select(r => r.point)
            //                         .ToList();
            
            if (enableDebug) $"Obstacle count: {repulsionPoints.Count}".Bold().Msg();
            float deltaOfTime = deltaTime;
            repulsionPoints.ForEach(p =>
            {
                //! The closer the point, the higher the repulsion multiplier
                float dist = (pilotTrans.position - p).magnitude;
                float multiplier = 1f;
                if (dist < obstacleDetectRadius)
                    multiplier += ((obstacleDetectRadius - dist) / obstacleDetectRadius) * closeRepulsionForce;

                //! Diversion force added if the AI is looking directly at the repulsion point
                Vector3 force = (pilotTrans.position - p).normalized * avoidanceForce * multiplier;
                Vector3 cross = Vector3.Cross(force.normalized, rBody.velocity.normalized);
                if (cross.magnitude.Abs() <= 0.2f)
                {
                    if (enableDebug) "parralel".Msg();
                    Vector3 direction = force.normalized;

                    //! Diversion force added is always towards the right
                    Vector3 relativeRight = new Vector3(direction.z, direction.y, -direction.x).normalized;
                    force += relativeRight * axisStalemateDeviationForce;
                }
                rBody.AddForce(force * deltaOfTime, ForceMode.VelocityChange);
            });

            repulsionPoints.Clear();

            // var defaultRepulsionPoints = navPoints.Select(n => n.GetPosition).Where(p => Vector3.Distance(p, pilotTrans.position) <= obstacleDetectRadius).ToList();
            // defaultRepulsionPoints.ForEach(point =>
            // {
            //     //! The closer the point, the higher the repulsion multiplier
            //     float dist = (pilotTrans.position - point).magnitude;
            //     float multiplier = 1f;
            //     if (dist < obstacleDetectRadius)
            //         multiplier += ((obstacleDetectRadius - dist) / obstacleDetectRadius) * closeRepulsionForce;

            //     //! Diversion force added if the AI is looking directly at the repulsion point
            //     Vector3 force = (pilotTrans.position - point).normalized * avoidanceForce * multiplier;
            //     Vector3 cross = Vector3.Cross(force.normalized, rBody.velocity.normalized);
            //     if (cross.magnitude.Abs() <= 0.5f)
            //     {
            //         if (enableDebug) "parralel".Msg();
            //         Vector3 direction = force.normalized;

            //         //! Diversion force added is always towards the right
            //         Vector3 relativeRight = new Vector3(direction.z, direction.y, -direction.x).normalized;
            //         force += relativeRight * axisStalemateDeviationForce;
            //     }
            //     rBody.AddForce(force, ForceMode.VelocityChange);
            // });

        }

        private void TrySelectNewNavPoint(in float deltaTime)
        {
            if (!canAutoSelectNavPoints) return;

            if (hasReachedPoint)
                lingerTimer -= deltaTime;
            else
                timeoutTimer -= deltaTime;

            if (lingerTimer <= 0f || timeoutTimer <= 0f)
            {
                ResetLingerTimer();
                ResetTimeoutTimer();
                StopCustomPath();
                SelectNewNavPoint();
            }
        }

        private void SelectNewNavPoint()
        {
            var points = navPoints.Where(o => o != currentPoint);
            List<NavPoint> potentialPoints = points.OrderBy(n => n.GetSqrDistanceTo(currentPoint.GetPosition)).Take(numberOfClosestPointsToConsider).ToList();
            if (currentPoint != null) currentPoint.Deselect();
			currentPoint = potentialPoints.RandomElement();
			currentPoint.Select();
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

            /*
            float closeRadius = obstacleDetectRadius + 2f;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(pilotTrans.position, closeRadius * closeRadius);

            if (currentPoint == null) return;
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(pilotTrans.position, currentPoint.GetSqrDistanceTo(pilotTrans.position));
            */
        }
    }

    [System.Serializable]
    public class CollisionPoint
    {
        public Collider Collider { get; private set; }
        public Vector3 Point { get; private set; }

        public CollisionPoint(Collider collider, Vector3 point)
        {
            Collider = collider;
            Point = point;
        }
    }
}
