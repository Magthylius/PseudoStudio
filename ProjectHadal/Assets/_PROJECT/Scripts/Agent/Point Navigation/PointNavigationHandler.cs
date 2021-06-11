using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using NaughtyAttributes;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;
using ReadOnlyAttribute = Tenshi.ReadOnlyAttribute;

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

        [Header("Force Settings")]
        [SerializeField] private float maxVelocity;
        [SerializeField] private float thrustForce;
        [SerializeField] private float additionalBoostThrustForce;
        [SerializeField] private float attractionForce;
        [SerializeField] private float avoidanceForce;
        [SerializeField] private float closeRepulsionForce;
        [SerializeField] private float axisStalemateDeviationForce;
        [SerializeField] private float obstacleDetectRadius;
        [SerializeField] private float smoothLookAtSpeed;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Nav Components")]
        [SerializeField, Range(2, 10)] private int numberOfClosestPointsToConsider;
        [SerializeField] private Transform pilotTrans;
        [SerializeField] private Rigidbody rBody;

        [Header("Internal Data")]
        [SerializeField, ReadOnly] private float obstacleCheckTimer;
        [SerializeField, ReadOnly] private float timeoutTimer;
        [SerializeField, ReadOnly] private float lingerTimer;
        [Space(10)]
        [SerializeField, ReadOnly] private float speedMultiplier = 1f;
        [SerializeField, ReadOnly] private List<NavPoint> navPoints;
        [SerializeField, ReadOnly] private List<Vector3> repulsionPoints;
        [SerializeField, ReadOnly] private bool hasReachedPoint;
        [SerializeField, ReadOnly] private bool canTimeout;
        [SerializeField, ReadOnly] private bool canAutoSelectNavPoints;
        [SerializeField, ReadOnly] private bool isOnCustomPath;
        [SerializeField, ReadOnly] private bool isChasingAPlayer;
        [SerializeField, ReadOnly] private bool canPath;
        [SerializeField, ReadOnly] private NavPoint currentPoint;
        private Queue<NavPoint> pointPath;

        // private void Awake() { Initialise(); }
        // private void Update() { DoUpdate(DeltaTime); }
        // private void FixedUpdate() { DoFixedUpdate(FixedDeltaTime); }


        #region Public Methods

        public void Initialise()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            navPoints.ForEach(p => p.Initialise());
            ResetLingerTimer();
            ResetTimeoutTimer();
            obstacleCheckTimer = 0f;
            hasReachedPoint = false;
            canTimeout = true;
            currentPoint = null;
            canAutoSelectNavPoints = true;
            isOnCustomPath = false;
            isChasingAPlayer = false;
            canPath = true;
            if (rBody == null) rBody = GetComponentInParent<Rigidbody>();
            if (numberOfClosestPointsToConsider > navPoints.Count - 1) numberOfClosestPointsToConsider = navPoints.Count - 1;
            currentPoint = GetClosestPointToSelf();
            repulsionPoints = new List<Vector3>();
            pointPath = new Queue<NavPoint>();
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
        public float TotalThrustForce => (thrustForce + (isChasingAPlayer.AsFloat() * additionalBoostThrustForce)) * speedMultiplier;
        public Transform PilotTransform => pilotTrans;

        public void SetSpeedMultiplier(in float multiplier) => speedMultiplier = multiplier.Clamp(0.1f, float.MaxValue);
        public void ResetSpeedMultiplier() => SetSpeedMultiplier(1f);

        public void AddRepulsionPoint(Vector3 point)
        {
            if (!ObstacleTimerReached || repulsionPoints.Contains(point))
                return;
            
            repulsionPoints.Add(point);
        }

        public List<Vector3> GetRepulsionPoints() => new List<Vector3>(repulsionPoints);

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
            if (rBody != null)
                rBody.velocity = Vector3.zero;
        }

        /// <summary>
        /// Plans out a path to the destination cavern.
        /// </summary>
        /// <param name="manager">Used to obtain cavern-related information.</param>
        /// <param name="destination">The destination where the pilot should end up.</param>
        public void SetDestinationToCavern(CavernManager manager, CavernHandler destination)
        {
            if (manager == null || destination == null)
            {
                if (enableDebug) "CavernManager or Destination cavern is null.".Msg();
                return;
            }

            Vector3 curPointPos = currentPoint.GetPosition;
            CavernHandler currentCavern = manager.GetHandlerOfAILocation;
            NavPoint[] entryPoints = currentCavern.GetEntryNavPoints(destination);

            NavPoint first = entryPoints.Where(point => point.CavernTag == currentCavern.cavernTag).Single();
            NavPoint second = (entryPoints[0] == first) ? entryPoints[1] : entryPoints[0];
            NavPoint third = navPoints
                        .Where(point => point.CavernTag == destination.cavernTag && point != second)
                        .OrderBy(point => point.GetSqrDistanceTo(curPointPos))
                        .Take(numberOfClosestPointsToConsider - 1)
                        .RandomElement();

            if (first == null || second == null || third == null)
            {
                if (enableDebug) "A point for the queue is missing.".Msg();
                return;
            }

            Queue<NavPoint> points = new Queue<NavPoint>();
            points.Enqueue(first);
            points.Enqueue(second);
            points.Enqueue(third);

            SetQueuedPath(points);
        }

        /// <summary>
        /// Sets the current point to a custom nav point (note that this must be a freshly instantiated nav point).
        /// When the pilot reaches the custom destination, the nav point will be deleted with Destroy().
        /// </summary>
        /// <param name="target">An instantiated nav point that can be sitting at a fixed location, or parented to a gameObject.</param>
        /// <param name="targetIsPlayer">If the nav point is specified attached to the player, it will alter the pilot movements.</param>
        public void SetCustomPath(NavPoint target, bool targetIsPlayer)
        {
            if (target == null) return;
            isOnCustomPath = true;
            hasReachedPoint = false;
            pointPath.Clear();
            currentPoint = target;
            isChasingAPlayer = targetIsPlayer;
            canTimeout = false;
            canAutoSelectNavPoints = !targetIsPlayer;
            ResetLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug) "Setting custom path".Msg();
        }

        /// <summary>
        /// Similar to the other SetCustomPath() function, but this accepts a queue of nav points to plan out a path. These
        /// nav points must be freshly instantiated and will be deleted with Destroy() when the pilot reaches each point.
        /// </summary>
        /// <param name="points">The queue that will be used to plot out a path plan.</param>
        public void SetCustomPath(Queue<NavPoint> points)
        {
            if (points.IsNullOrEmpty())
                return;

            isOnCustomPath = true;
            hasReachedPoint = false;
            pointPath = new Queue<NavPoint>(points);
            currentPoint = pointPath.Dequeue();
            isChasingAPlayer = false;
            canTimeout = false;
            canAutoSelectNavPoints = false;
            ResetLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug) "Setting custom paths in queue".Msg();
        }

        /// <summary>
        /// Accepts a queue of existing nav points in the game to plan out a path. The nav points used must be existing in the
        /// scene and not freshly instantiated as they will not be deleted after the pilot reaches each point.
        /// </summary>
        /// <param name="pointsArray">The array of NavPoints that will be used to plot out a path plan.</param>
        public void SetQueuedPath(NavPoint[] pointsArray)
        {
            Queue<NavPoint> newQueue = new Queue<NavPoint>();
            foreach(NavPoint points in pointsArray)
                newQueue.Enqueue(points);
            
            SetQueuedPath(newQueue);
        }
        
        /// <summary>
        /// Accepts a queue of existing nav points in the game to plan out a path. The nav points used must be existing in the
        /// scene and not freshly instantiated as they will not be deleted after the pilot reaches each point.
        /// </summary>
        /// <param name="points">The queue of NavPoints that will be used to plot out a path plan.</param>
        public void SetQueuedPath(Queue<NavPoint> points)
        {
            if (points.IsNullOrEmpty())
                return;

            isOnCustomPath = false;
            hasReachedPoint = false;
            pointPath = new Queue<NavPoint>(points);
            currentPoint = pointPath.Dequeue();
            isChasingAPlayer = false;
            canTimeout = false;
            canAutoSelectNavPoints = false;
            ResetLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug) "Setting queued path".Msg();
        }

        /// <summary>
        /// If there is no point path queue, it will ask the handler to immediately find a new point in the cavern. Otherwise, it will
        /// skip the current point in the queue and move on to the next point immediately.
        /// </summary>
        public void SkipCurrentPath()
        {
            currentPoint.Deselect();
            if (currentPoint.CavernTag == CavernTag.Custom_Point)
                Destroy(currentPoint.gameObject);
            SelectNewNavPoint();
        }

        /// <summary>
        /// Stops the pilot from continuing to go towards the custom point. If it was a custom queue path, the entire queue will be
        /// discarded and the handler will assume normal pathing afterward.
        /// </summary>
        /// <param name="instantlyFindNewNavPoint">If false, it will just get the closest existing nav point to sync to.</param>
        public void StopCustomPath(bool instantlyFindNewNavPoint = false)
        {
            canAutoSelectNavPoints = true;
            if (isOnCustomPath)
            {
                isOnCustomPath = false;
                pointPath.Clear();
                StartCoroutine(DestroyAndRegenerateCurrentNavPoint(instantlyFindNewNavPoint));
            }

            IEnumerator DestroyAndRegenerateCurrentNavPoint(bool justFindNewPoint)
            {
                currentPoint.Deselect();
                if (currentPoint.CavernTag == CavernTag.Custom_Point)
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
            Vector3 force = pilotTrans.forward * (TotalThrustForce * deltaTime);
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
                EvaluateQueuedPath();
                if (enableDebug) "Point Reached".Msg();
            }

            void EvaluateQueuedPath()
            {
                if (pointPath.IsNotEmpty())
                {
                    currentPoint.Deselect();
                    if (isOnCustomPath)
                        Destroy(currentPoint.gameObject);

                    currentPoint = pointPath.Dequeue();
                    hasReachedPoint = false;
                    return;
                }
                
                canTimeout = true;
                canAutoSelectNavPoints = true;
            }
        }

        private void HandleObstacleAvoidance(in float deltaTime)
        {
            obstacleCheckTimer -= deltaTime;
            if (!ObstacleTimerReached) return;

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
        }

        /// <summary> Elapses timer to determine if it is time to move on to a new point. </summary>
        private void TrySelectNewNavPoint(in float deltaTime)
        {
            if (!canAutoSelectNavPoints) return;

            if (hasReachedPoint)
                lingerTimer -= deltaTime;
            else
            {
                if (canTimeout)
                    timeoutTimer -= deltaTime;
            }

            if (lingerTimer <= 0f || timeoutTimer <= 0f)
            {
                ResetLingerTimer();
                ResetTimeoutTimer();
                SkipCurrentPath();
            }
        }

        /// <summary> The actual logic that selects a new nav point. The algorithm behaviour can be adjusted with
        /// the <see cref="numberOfClosestPointsToConsider"/> variable. </summary>
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
        public bool ObstacleTimerReached => obstacleCheckTimer <= 0f;

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
