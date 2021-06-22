using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using Hadal.AI.Settings;
using Tenshi;
using Tenshi.UnitySoku;
using Tenshi.Shrine;
using UnityEngine;
using ReadOnlyAttribute = Tenshi.ReadOnlyAttribute;
using Random = UnityEngine.Random;

namespace Hadal.AI
{
    public enum SteeringMode
    {
        Cavern = 0,
        Tunnel,
        Stunned
    }

    public class PointNavigationHandler : MonoBehaviour, IUnityServicer
    {
        #region Data Accessors

        [Header("Internal Data")]
        [SerializeField, ReadOnly] private float obstacleCheckTimer;
        public float Data_ObjstacleCheckTimer => obstacleCheckTimer;
        [SerializeField, ReadOnly] private float timeoutTimer;
        public float Data_TimeoutTimer => timeoutTimer;
        [SerializeField, ReadOnly] private float navPointLingerTimer;
        public float Data_NavPointLingerTimer => navPointLingerTimer;
        [SerializeField, ReadOnly] private float cavernLingerTimer;
        public float Data_CavernLingerTimer => cavernLingerTimer;
        [SerializeField, ReadOnly] private float speedMultiplier = 1f;
        public float Data_SpeedMultiplier => speedMultiplier;
        [SerializeField, ReadOnly] private List<NavPoint> navPoints;
        public List<NavPoint> Data_NavPoints => navPoints;
        [SerializeField, ReadOnly] private List<Vector3> repulsionPoints;
        public List<Vector3> Data_RepulsionPoints => repulsionPoints;
        [SerializeField, ReadOnly] private bool hasReachedPoint;
        public bool Data_HasReachedPoint => hasReachedPoint;
        [SerializeField, ReadOnly] private bool canTimeout;
        public bool Data_CanTimeOut => canTimeout;
        [SerializeField, ReadOnly] private bool canAutoSelectNavPoints;
        public bool Data_CanAutoSelectNavPoints => canAutoSelectNavPoints;
        [SerializeField, ReadOnly] private bool isOnCustomPath;
        public bool Data_IsOnCustomPath => isOnCustomPath;
        [SerializeField, ReadOnly] private bool isOnQueuePath;
        public bool Data_IsOnQueuePath => isOnQueuePath;
        [SerializeField, ReadOnly] private bool isChasingAPlayer;
        public bool Data_IsChasingAPlayer => isChasingAPlayer;
        [SerializeField, ReadOnly] private bool canPath;
        public bool Data_CanPath => canPath;

        private float slowMultiplier = 0f;
        public void SetSlowMultiplier(float mult) => slowMultiplier = mult;

        private NavPoint CurrentPoint;
        private NavPoint currentPoint
        {
            get => CurrentPoint;
            set
            {
                CurrentPoint = value;
                if (enableDebug) print("Current point changed: " + value);
            }
        }
        public NavPoint Data_CurrentPoint => currentPoint;
        #endregion

        [Header("Debug")]
        [SerializeField] private bool enableDebug;
        [SerializeField] private bool showObstacleInfo;
        [SerializeField] private bool enableMovement;

        [Header("Timer Settings")]
        [SerializeField, MinMaxSlider(1f, 30f)] private Vector2 navPointLingerTimeRange;
        [SerializeField, MinMaxSlider(1f, 120f)] private Vector2 cavernLingerTimeRange;
        [SerializeField] private float timeoutNewPointTime;
        [SerializeField] private float obstacleCheckTime;

        [Header("Steering Settings")]
        [SerializeField] private SteeringMode _steeringMode;
        [SerializeField] private AISteeringSettings cavernSteeringSettings;
        [SerializeField] private AISteeringSettings tunnelSteeringSettings;
        [SerializeField] private AISteeringSettings stunnedSteeringSettings;
        [SerializeField, ReadOnly] private float maxVelocity;
        [SerializeField, ReadOnly] private float thrustForce;
        [SerializeField, ReadOnly] private float additionalAttractionForce;
        [SerializeField, ReadOnly] private float attractionForce;
        [SerializeField, ReadOnly] private float avoidanceForce;
        [SerializeField, ReadOnly] private float closeRepulsionForce;
        [SerializeField, ReadOnly] private float axisStalemateDeviationForce;
        [SerializeField, ReadOnly] private float obstacleDetectRadius;
        [SerializeField, ReadOnly] private float closeNavPointDetectionRadius;
        [SerializeField, ReadOnly] private float smoothLookAtSpeed;
        [SerializeField, ReadOnly] private LayerMask obstacleMask;
        [SerializeField, ReadOnly] private bool isStunned;

        private float debugVelocityMultiplier = 1f;

        [Header("Nav Components")]
        [SerializeField, Range(2, 10)] private int numberOfClosestPointsToConsider;
        [SerializeField] private Transform pilotTrans;
        [SerializeField] private Rigidbody rBody;
        private CavernManager cavernManager;

        //! Misc variables
        private Queue<NavPoint> pointPath;
        private Queue<NavPoint> cachedPointPath;
        private bool _isEnabled;
        private bool _tickCavernLingerTimer;
        public event Action<float> OnObstacleDetectRadiusChange;
        public event Action OnReachedPoint;

        private void OnValidate()
        {
            if (!enableDebug) showObstacleInfo = false;
        }

        private void OnDrawGizmos()
        {
            if (!enableDebug || pilotTrans == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pilotTrans.position, obstacleDetectRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pilotTrans.position, closeNavPointDetectionRadius);

            if (currentPoint == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pilotTrans.position, currentPoint.GetPosition);
        }


        #region Public Methods

        public void Initialise()
        {
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            navPoints.ForEach(p => p.Initialise());
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            obstacleCheckTimer = 0f;
            hasReachedPoint = false;
            canTimeout = true;
            currentPoint = null;
            canAutoSelectNavPoints = true;
            isOnCustomPath = false;
            isOnQueuePath = false;
            isChasingAPlayer = false;
            canPath = true;
            slowMultiplier = 0f;
            if (rBody == null) rBody = GetComponentInParent<Rigidbody>();
            if (numberOfClosestPointsToConsider > navPoints.Count - 1) numberOfClosestPointsToConsider = navPoints.Count - 1;
            currentPoint = GetClosestPointToSelf();
            repulsionPoints = new List<Vector3>();
            pointPath = new Queue<NavPoint>();
            cachedPointPath = new Queue<NavPoint>();
            _isEnabled = true;
            _tickCavernLingerTimer = false;

            CavernModeSteering();
			ResetCavernLingerTimer();
        }
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!CanMove || !canPath || !enableMovement) return;
            TrySelectNewNavPoint(fixedDeltaTime);
            ElapseCavernLingerTimer(fixedDeltaTime);
            MoveForwards(fixedDeltaTime);
            if (!isStunned) MoveTowardsCurrentNavPoint(fixedDeltaTime);
            HandleObstacleAvoidance(fixedDeltaTime);
            HandleSpeedAndDirection(fixedDeltaTime);
            ClampMaxVelocity();
        }

        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
        public float ObstacleDetectionRadius => obstacleDetectRadius;
        public float TotalThrustForce => thrustForce * speedMultiplier;
        public float TotalAttractionForce => attractionForce + (isChasingAPlayer.AsFloat() * additionalAttractionForce);
        public bool ObstacleTimerReached => obstacleCheckTimer <= 0f;
        public LayerMask GetObstacleMask => obstacleMask;
        /// <summary> Returns the pilot that this handler is running. </summary>
        public Transform PilotTransform => pilotTrans;

        /// <summary> Enables this component safely. </summary>
        public void Enable() => _isEnabled = true;
        /// <summary> Disables this component safely. </summary>
        public void Disable() => _isEnabled = false;
        public void SetCavernManager(CavernManager manager) => cavernManager = manager;
        public void SetSpeedMultiplier(in float multiplier) => speedMultiplier = multiplier.Clamp(0.1f, float.MaxValue);
        /// <summary> Resets speed multiplier value back to 1f. </summary>
        public void ResetSpeedMultiplier() => SetSpeedMultiplier(1f);
        public List<Vector3> GetRepulsionPoints() => new List<Vector3>(repulsionPoints);
        public void AddRepulsionPoint(Vector3 point)
        {
            if (ObstacleTimerReached && !repulsionPoints.Contains(point))
                repulsionPoints.Add(point);
        }

        /// <summary>
        /// Sets whether the handler will allow pathing for the pilot.
        /// </summary>
        /// <param name="statement">True or False</param>
        public void SetCanPath(bool statement)
        {
            canPath = statement;

            if (canPath)
            {
                ResetNavPointLingerTimer();
                ResetTimeoutTimer();
                obstacleCheckTimer = 0f;
                return;
            }
            if (rBody != null)
                rBody.velocity = Vector3.zero;
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
            canTimeout = true;
            canAutoSelectNavPoints = !targetIsPlayer;
            print("custom: " + canAutoSelectNavPoints);
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug) "Setting custom nav point path".Msg();
            
        }

        public void ComputeCachedDestinationCavernPath(CavernHandler destination)
        {
            if (cavernManager == null || destination == null)
            {
                if (enableDebug) "CavernManager or Destination cavern is null.".Msg();
                return;
            }
            
            CavernHandler currentCavern = cavernManager.GetHandlerOfAILocation;
            NavPoint[] entryPoints = currentCavern.GetEntryNavPoints(destination);

            cachedPointPath.Clear();

            //! First point and its approach points. Enqueue approach first.
            NavPoint first = entryPoints.Single(point => point.CavernTag == currentCavern.cavernTag);
            
            NavPoint appChild = first.approachPoint;
            while (appChild != null)
            {
                cachedPointPath.Enqueue(appChild);
                appChild = appChild.approachPoint;
            }

            //! Approach points need to reversed so that FirstInLastOut
            cachedPointPath = new Queue<NavPoint>(cachedPointPath.Reverse());
            cachedPointPath.Enqueue(first);

            //! Second point and its approach points. Enqueue exit first.
            NavPoint second = (entryPoints[0] == first) ? entryPoints[1] : entryPoints[0];
            cachedPointPath.Enqueue(second);

            appChild = second.approachPoint;
            while (appChild != null)
            {
                cachedPointPath.Enqueue(appChild);
                appChild = appChild.approachPoint;
            }


            var potentialList = navPoints
                        .Where(point => HasTheSameCavernTagAsDestinationCavern(point) && IsNotTheSamePoint(point, second) && !point.IsTunnelEntry)
                        .ToList()
                        .Shuffle(Time.frameCount)
                        .Take(numberOfClosestPointsToConsider)
                        .Where(p => p != null)
                        .ToList();

            if (potentialList.IsEmpty())
            {
                if (enableDebug) "No potential points found for third cavern point.".Msg();
                return;
            }
            NavPoint third = potentialList.RandomElement();

            if (cachedPointPath.Contains(null))
            {
                if (enableDebug) "A point for the queue is missing or null.".Msg();
                return;
            }

            cachedPointPath.Enqueue(third);

            if (enableDebug)
            {
                //$"Created cached Queued Path: {first.gameObject.name}, {second.gameObject.name}, {third.gameObject.name}".Msg();
                string pathQueue = "Created cached queued path: ";
                foreach (NavPoint point in cachedPointPath)
                    pathQueue += point.gameObject.name + ", ";
            }

            // Local Methods
            bool HasTheSameCavernTagAsDestinationCavern(NavPoint point) => point && point.CavernTag == destination.cavernTag;
            bool IsNotTheSamePoint(NavPoint point, NavPoint other) => point && point != other;
        }

        public void EnableCachedQueuePathTimer() => _tickCavernLingerTimer = true;

        /// <summary>
        /// Computes a plan for the path to a destination cavern and immediately follows it. It will return if the handler is
        /// currently chasing a player.
        /// </summary>
        /// <param name="destination">The destination where the pilot should end up.</param>
        public void SetImmediateDestinationToCavern(CavernHandler destination)
        {
            if (isChasingAPlayer) return;
            if (cavernManager == null || destination == null)
            {
                if (enableDebug) "CavernManager or Destination cavern is null.".Msg();
                return;
            }
            
            ComputeCachedDestinationCavernPath(destination);
            SetQueuedPathFromCache();
        }

        /// <summary>
        /// Sets the handler's path to use the cached queue path that has been generated. If there is no cache queue path that has
        /// been generated beforehand, the function will return. See <see cref="ComputeCachedDestinationCavernPath"/>.
        /// </summary>
        public void SetQueuedPathFromCache()
        {
            if (cachedPointPath.IsNullOrEmpty())
            {
                if (enableDebug) $"There is no cached point path to run. Please compute the cached path before calling this.".Msg();
                return;
            }
            if (enableDebug) "Running cached queue path...".Msg();
            SetQueuedPath(cachedPointPath);
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
            isOnQueuePath = true;
            isChasingAPlayer = false;
            canTimeout = false;
            canAutoSelectNavPoints = false;
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug)
            {
                string debugPath = "";
                foreach (NavPoint point in pointPath)
                    debugPath += point + ",";

                $"Queued path set: {debugPath}".Msg();
            }
        }

        /// <summary>
        /// If there is no point path queue and the parameter is set to True, it will ask the handler to immediately find a new point
        /// in the cavern. Otherwise, it will skip the current point in the queue and move on to the next point in the next update frame.
        /// </summary>
        public void SkipCurrentPoint(bool automaticallySelectNewPoint)
        {
            if (currentPoint == null)
            {
                SelectNewNavPoint();
                return;
            }

            currentPoint.Deselect();
            if (currentPoint.CavernTag == CavernTag.Custom_Point) Destroy(currentPoint.gameObject);
            if (automaticallySelectNewPoint) SelectNewNavPoint();
            if (enableDebug) $"New point selected: {currentPoint.gameObject.name}".Msg();
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

                //Debug.LogWarning("start coroutine: " + currentPoint.CavernTag);
                if (isChasingAPlayer)
                {
                    isChasingAPlayer = false;
                    Destroy(currentPoint.gameObject);
                }

                if (enableDebug) "Stopping custom path".Msg();

                if (justFindNewPoint)
                {
                    ResetNavPointLingerTimer();
                    ResetTimeoutTimer();
                    SelectNewNavPoint();
                    yield break;
                }
                currentPoint = GetClosestPointToSelf();
            }
        }

        public void TunnelModeSteering()
        {
            _steeringMode = SteeringMode.Tunnel;
            UpdateSteering();
        }

        public void CavernModeSteering()
        {
            _steeringMode = SteeringMode.Cavern;
            UpdateSteering();
        }

        public void StunnedModeSteering()
        {
            _steeringMode = SteeringMode.Stunned;
            UpdateSteering();
        }

        private void UpdateSteering()
        {
            AISteeringSettings currentSteer = cavernSteeringSettings;

            if (_steeringMode == SteeringMode.Cavern)
                currentSteer = cavernSteeringSettings;
            else if (_steeringMode == SteeringMode.Tunnel)
                currentSteer = tunnelSteeringSettings;
            else if (_steeringMode == SteeringMode.Stunned)
                currentSteer = stunnedSteeringSettings;

            maxVelocity = currentSteer.MaxVelocity;
            thrustForce = currentSteer.ThrustForce;
            additionalAttractionForce = currentSteer.AdditionalAttractionForce;
            attractionForce = currentSteer.AttractionForce;
            avoidanceForce = currentSteer.AvoidanceForce;
            closeRepulsionForce = currentSteer.CloseRepulsionForce;
            axisStalemateDeviationForce = currentSteer.AxisStalemateDeviationForce;
            obstacleDetectRadius = currentSteer.ObstacleDetectRadius;
            closeNavPointDetectionRadius = currentSteer.CloseNavPointDetectionRadius;
            smoothLookAtSpeed = currentSteer.SmoothLookAtSpeed;
            obstacleMask = currentSteer.ObstacleMask;
            FindObjectOfType<AIBrain>().ChangeColliderMaterial(currentSteer.PhysicMaterial);

            OnObstacleDetectRadiusChange?.Invoke(obstacleDetectRadius);
        }

        public void SetDebugVelocityMultiplier(float multiplier) => debugVelocityMultiplier = multiplier;
        public void ResetDebugVelocityMultiplier() => SetDebugVelocityMultiplier(1f);
        #endregion

        #region Private Methods

        private void MoveForwards(in float deltaTime)
        {
            if (MaxVelocity == 0f) return;
            float speedDeduction = TotalThrustForce * slowMultiplier;
            Vector3 force = pilotTrans.forward * ((TotalThrustForce - speedDeduction) * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);
        }

        private void HandleSpeedAndDirection(in float deltaTime)
        {
            if (currentPoint == null) return;

            //! Chasing player direction
            if (isChasingAPlayer)
            {
                Vector3 moveTo = (currentPoint.GetPosition - pilotTrans.position).normalized * TotalAttractionForce;
                rBody.velocity = Vector3.Lerp(rBody.velocity, rBody.velocity + moveTo, deltaTime * attractionForce);
            }

            //! Look at
            pilotTrans.forward = Vector3.Lerp(pilotTrans.forward, rBody.velocity.normalized, deltaTime * smoothLookAtSpeed);
        }

        private void ClampMaxVelocity()
        {
            if (rBody.velocity.magnitude > MaxVelocity * (1f - slowMultiplier))
                rBody.velocity = rBody.velocity.normalized * MaxVelocity;
			
			if (MaxVelocity == 0f)
				rBody.velocity = Vector3.zero;
        }

        private void MoveTowardsCurrentNavPoint(in float deltaTime)
        {
            if (currentPoint == null || MaxVelocity == 0f) return;
            Vector3 direction = currentPoint.GetDirectionTo(pilotTrans.position);
            float speedDeduction = TotalAttractionForce * slowMultiplier;
            Vector3 force = direction * ((TotalAttractionForce - speedDeduction) * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);

            if (!hasReachedPoint && CloseEnoughToTargetNavPoint())
            {
                hasReachedPoint = true;
                OnReachedPoint?.Invoke();
                EvaluateQueuedPath();
                if (enableDebug) $"Point Reached: {currentPoint.gameObject.name}".Msg();
            }

            //! Local shorthands
            bool CloseEnoughToTargetNavPoint() => currentPoint.GetSqrDistanceTo(pilotTrans.position) < (closeNavPointDetectionRadius * closeNavPointDetectionRadius);

            void EvaluateQueuedPath()
            {
                if (pointPath.IsNotEmpty())
                {
                    currentPoint.Deselect();
                    if (isOnCustomPath)
                        Destroy(currentPoint.gameObject);

                    currentPoint = pointPath.Dequeue();
                    hasReachedPoint = false;
                    ResetTimeoutTimer();
                    return;
                }

                canTimeout = true;
                canAutoSelectNavPoints = true;
                isOnQueuePath = false;
                if (enableDebug) "Queued path is done.".Msg();
            }
        }

        private void HandleObstacleAvoidance(in float deltaTime)
        {
            obstacleCheckTimer -= deltaTime;
            if (!ObstacleTimerReached) return;
            if (enableDebug && showObstacleInfo) $"Obstacle count: {repulsionPoints.Count}".Msg();

            float deltaOfTime = deltaTime;
            repulsionPoints.ForEach(p =>
            {
                //! The closer the point, the higher the repulsion multiplier
                float dist = (pilotTrans.position - p).magnitude;
                float multiplier = 1f;
                if (dist < obstacleDetectRadius)
                    multiplier += ((obstacleDetectRadius - dist) / obstacleDetectRadius) * closeRepulsionForce;

                //! Diversion force added if the AI is looking directly at the repulsion point
                Vector3 force = (pilotTrans.position - p).normalized * (avoidanceForce * multiplier);
                if (!isChasingAPlayer)
                {
                    Vector3 cross = Vector3.Cross(force.normalized, rBody.velocity.normalized);
                    if (cross.magnitude.Abs() <= 0.2f)
                    {
                        Vector3 direction = force.normalized;

                        //! Diversion force added is always towards the right
                        Vector3 relativeRight = new Vector3(direction.z, direction.y, -direction.x).normalized;
                        force += relativeRight * axisStalemateDeviationForce;
                    }
                }
                float speedModifier = force.magnitude * (1f - slowMultiplier);
                force = force.normalized * speedModifier;
                rBody.AddForce(force * deltaOfTime, ForceMode.VelocityChange);
            });

            if (repulsionPoints.IsNotEmpty())
            {
                repulsionPoints.Clear();
                ResetObstacleCheckTimer();
            }
        }

        /// <summary> Elapses timer to determine if it is time to move on to a new point. </summary>
        private void TrySelectNewNavPoint(in float deltaTime)
        {
            if (!canAutoSelectNavPoints) return;

            if (hasReachedPoint)
                navPointLingerTimer -= deltaTime;
            else
            {
                if (canTimeout)
                    timeoutTimer -= deltaTime;
            }

            if (navPointLingerTimer <= 0f || timeoutTimer <= 0f)
            {
                ResetNavPointLingerTimer();
                ResetTimeoutTimer();
                SkipCurrentPoint(true);
            }
        }

        private void ElapseCavernLingerTimer(in float deltaTime)
        {
            if (!_tickCavernLingerTimer) return;

            cavernLingerTimer -= deltaTime;
            if (cavernLingerTimer > 0f) return;

            _tickCavernLingerTimer = false;
            ResetCavernLingerTimer();
            SetQueuedPathFromCache();
        }

        /// <summary> The actual logic that selects a new nav point. The algorithm behaviour can be adjusted with
        /// the <see cref="numberOfClosestPointsToConsider"/> variable. </summary>
        private void SelectNewNavPoint()
        {
            if (cavernManager == null)
                return;

            if (currentPoint == null)
                currentPoint = GetClosestPointToSelf();

            List<NavPoint> potentialPoints = navPoints
                                            .Where(o => o != null && o != currentPoint && o.CavernTag == cavernManager.GetCavernTagOfAILocation() && !o.IsTunnelEntry)
                                            .OrderBy(n => n.GetSqrDistanceTo(currentPoint.GetPosition))
                                            .Take(numberOfClosestPointsToConsider)
                                            .ToList();

            potentialPoints.RemoveAll(p => p == null);
            if (potentialPoints.IsEmpty())
            {
                if (enableDebug)
                    $"Potential points is empty; AI in {cavernManager.GetCavernTagOfAILocation()}".Msg();
                return;
            }

            if (currentPoint != null) currentPoint.Deselect();
            currentPoint = potentialPoints.RandomElement();
            currentPoint.Select();
            hasReachedPoint = false;
            if (enableDebug)
                $"Selected new point: {currentPoint.gameObject.name}; Brain current cavern: {cavernManager.GetCavernTagOfAILocation()}".Msg();
        }

        public Queue<NavPoint> GetPointPath => pointPath;
        public Queue<NavPoint> GetCachedPointPath => cachedPointPath;
        private NavPoint GetClosestPointToSelf() => navPoints.Where(n => n != null).OrderBy(n => n.GetSqrDistanceTo(pilotTrans.position)).FirstOrDefault();
        private void ResetTimeoutTimer() => timeoutTimer = timeoutNewPointTime;
        private void ResetObstacleCheckTimer() => obstacleCheckTimer = obstacleCheckTime;
        private void ResetNavPointLingerTimer() => navPointLingerTimer = GetNextNavPointLingerTime();
        private void ResetCavernLingerTimer() => cavernLingerTimer = GetNextCavernLingerTime();
        private float GetNextNavPointLingerTime() => Random.Range(navPointLingerTimeRange.x, navPointLingerTimeRange.y);
        private float GetNextCavernLingerTime() => Random.Range(cavernLingerTimeRange.x, cavernLingerTimeRange.y);

        public void SetAIStunned(bool isStun) => isStunned = isStun;

        #endregion

        #region Shorthands

        public float MaxVelocity => maxVelocity * debugVelocityMultiplier;
        private bool CanMove => _isEnabled && pilotTrans != null && rBody != null;

        #endregion
    }
}
