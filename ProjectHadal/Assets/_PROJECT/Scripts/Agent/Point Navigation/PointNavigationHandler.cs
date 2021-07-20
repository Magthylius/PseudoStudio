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
using NaughtyAttributes;
using ReadOnlyAttribute = Tenshi.ReadOnlyAttribute;
using Random = UnityEngine.Random;
using Photon.Pun;

namespace Hadal.AI
{
    public enum SteeringMode
    {
        Cavern = 0,
        Tunnel,
        Stunned,
        Engage
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
        [SerializeField, ReadOnly] private float lairCavernLingerTimer;
        public float Data_LairCavernLingerTimer => lairCavernLingerTimer;
        [SerializeField, ReadOnly] private float hydrothermalCavernLingerTimer;
        [SerializeField, ReadOnly] private float crystalCavernLingerTimer;
        [SerializeField, ReadOnly] private float biolumiCavernLingerTimer;
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
        [SerializeField, ReadOnly] private bool chosenAmbushPoint;
        public bool Data_ChosenAmbushPoint => chosenAmbushPoint;
        [SerializeField, ReadOnly] private bool lockSteeringBehaviour;
        public bool Data_LockSteeringBehaviour => lockSteeringBehaviour;
		
		[SerializeField, ReadOnly] private bool ignoreCavernLingerTimer = false;
		public bool Data_IgnoreCavernLingerTimer => ignoreCavernLingerTimer;
		public void SetIgnoreCavernLingerTimer(bool statement) => ignoreCavernLingerTimer = statement;

        private float slowMultiplier = 0f;
        public void SetSlowMultiplier(float mult)
        {
            if (mult >= 1.0f) return;
            slowMultiplier = mult;
        }

        public NavPoint GetCurrentPoint => currentPoint;
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

        private CavernTag cachedLatestCavernTag;
        public CavernTag Data_CachedLatestCavernTag => cachedLatestCavernTag;
        public void UpdateLatestCavernTag(CavernTag newCurrentTag, CavernTag nextCavernTagIfAny)
        {
            if (newCurrentTag == CavernTag.Invalid)
                return;

            bool pickNextCavern = TenshiMath.HeadsOrTails();
            if (pickNextCavern && nextCavernTagIfAny != CavernTag.Invalid)
            {
                cachedLatestCavernTag = nextCavernTagIfAny;
                return;
            }
            cachedLatestCavernTag = newCurrentTag;
        }

        #endregion

        [Header("Debug")]
        [SerializeField] private bool enableDebug;
        [SerializeField] private bool showObstacleInfo;
        [SerializeField] private bool enableMovement;
        [SerializeField] private bool disableOnStart;

        [Header("Timer Settings")]
        [SerializeField, MinMaxSlider(1f, 30f)] private Vector2 navPointLingerTimeRange;
        [SerializeField, MinMaxSlider(1f, 120f)] private Vector2 crystalCavernLingerTimeRange;
        [SerializeField, MinMaxSlider(1f, 120f)] private Vector2 lairCavernLingerTimeRange;
        [SerializeField, MinMaxSlider(1f, 120f)] private Vector2 hydrothermalCavernLingerTimeRange;
        [SerializeField, MinMaxSlider(1f, 120f)] private Vector2 biolumiCavernLingerTimeRange;
        [SerializeField] private float timeoutNewPointTime;
        [SerializeField] private float obstacleCheckTime;

        [Header("Steering Settings")]
        [SerializeField] private SteeringMode _steeringMode;
        [SerializeField] private AISteeringSettings cavernSteeringSettings;
        [SerializeField] private AISteeringSettings tunnelSteeringSettings;
        [SerializeField] private AISteeringSettings stunnedSteeringSettings;
        [SerializeField] private AISteeringSettings engagementSteeringSettings;
        private AISteeringSettings currentSteer;
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
        [SerializeField, Range(1, 3)] private int numberOfClosestPointsToConsiderAfterTunnelExit;
        [SerializeField] private bool shuffleTunnelExitPoint;
        [SerializeField] private Transform pilotTrans;
        [SerializeField] private Rigidbody rBody;
        private CavernManager cavernManager;

        //! Misc variables
        private Queue<NavPoint> pointPath;
        private Queue<NavPoint> cachedPointPath;
        private bool _isEnabled;
        private bool _tickCavernLingerTimer;
        private Coroutine disableRoutine;
        private Transform _lookAtTarget = null;
        public event Action<float> OnObstacleDetectRadiusChange;
        public event Action OnReachedPoint;

        private void OnValidate()
        {
            if (!enableDebug) showObstacleInfo = false;
        }

        private void OnDestroy()
        {

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
            //! Get all navpoints in scene & intialise
            navPoints = FindObjectsOfType<NavPoint>().ToList();
            navPoints.ForEach(p => p.Initialise());

            //! Timers
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            obstacleCheckTimer = 0f;

            //! Runtime data
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

            if (disableOnStart)
                Disable();
        }
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!CanMove || !canPath || !enableMovement) return;

            if (!isStunned && (!hasReachedPoint || !chosenAmbushPoint))
            {
                TrySelectNewNavPoint(fixedDeltaTime);
                ElapseCavernLingerTimer(fixedDeltaTime);
                MoveForwards(fixedDeltaTime);
                MoveTowardsCurrentNavPoint(fixedDeltaTime);
                HandleObstacleAvoidance(fixedDeltaTime);
            }

            HandleSpeedAndDirection(fixedDeltaTime);
            ClampMaxVelocity();
        }

        public Func<bool> OnCollisionDetected() => () =>
        {
            if (!isStunned)
                return false;

            if (rBody != null)
                rBody.velocity = Vector3.zero;

            return true;
        };

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
		[Button(nameof(Enable))]
        public void Enable()
        {
            if (disableRoutine != null)
                StopCoroutine(disableRoutine);

            disableRoutine = null;
            _isEnabled = true;
            if (rBody != null) rBody.isKinematic = false;
        }
        /// <summary> Disables this component safely. </summary>
		[Button(nameof(Disable))]
        public void Disable(bool makeKinematic = true)
        {
            _isEnabled = false;
            if (rBody != null)
            {
                if (makeKinematic) rBody.isKinematic = true;
                rBody.velocity = Vector3.zero;
            }
        }
        public void ForceDisable(bool makeKinematic = true)
        {
            _isEnabled = false;
            if (disableRoutine != null)
                StopCoroutine(disableRoutine);
            disableRoutine = null;

            if (rBody != null)
            {
                if (makeKinematic) rBody.isKinematic = true;
                rBody.velocity = Vector3.zero;
            }
        }
        public void DisableWithLerp(float time, Action onCompleteCallback = null, float minVelocityCut = 0f)
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;
            if (rBody != null)
                disableRoutine = StartCoroutine(LerpVelocity());

            IEnumerator LerpVelocity()
            {
                float percent = 0f;
                Vector3 target;
                if (minVelocityCut <= 0f)
                    target = Vector3.zero;
                else
                    target = rBody.velocity * minVelocityCut;

                while (percent < 1f)
                {
                    float delta = DeltaTime * time;
                    percent += delta;
                    rBody.velocity = Vector3.Lerp(rBody.velocity, target, percent);
                    yield return null;
                }
                rBody.isKinematic = minVelocityCut <= 0f;
                rBody.velocity = target;
                onCompleteCallback?.Invoke();
            }
        }
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

        public void SetLookAtTarget(Transform targetTrans) => _lookAtTarget = targetTrans;

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
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            if (targetIsPlayer)
            {
                EngageModeSteering();
            }
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
            if (currentCavern.cavernTag == destination.cavernTag)
                return;

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


            List<NavPoint> potentialList = new List<NavPoint>();

            if (shuffleTunnelExitPoint)
            {
                potentialList = navPoints
                        .Where(point => HasTheSameCavernTagAsDestinationCavern(point) && IsNotTheSamePoint(point, second) && !point.IsTunnelEntry)
                        .ToList()
                        .Shuffle(Time.frameCount)
                        .Take(numberOfClosestPointsToConsiderAfterTunnelExit)
                        .Where(p => p != null)
                        .ToList();
            }
            else
            {
                Vector3 position = pilotTrans.position;
                potentialList = navPoints
                        .Where(point => HasTheSameCavernTagAsDestinationCavern(point) && IsNotTheSamePoint(point, second) && !point.IsTunnelEntry)
                        .OrderBy(p => p.GetSqrDistanceTo(position))
                        .Take(numberOfClosestPointsToConsiderAfterTunnelExit)
                        .Where(p => p != null)
                        .ToList();
            }

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
                string pathQueue = "Created cached queued path: ";
                foreach (NavPoint point in cachedPointPath)
                    pathQueue += point.Name + ", ";

                pathQueue.Msg();
            }

            // Local Methods
            bool HasTheSameCavernTagAsDestinationCavern(NavPoint point) => point && point.CavernTag == destination.cavernTag;
            bool IsNotTheSamePoint(NavPoint point, NavPoint other) => point && point != other;
        }

        public void EnableCachedQueuePathTimer()
        {
            if (cachedPointPath.IsNullOrEmpty())
                return;
            _tickCavernLingerTimer = true;
        }

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
            SetQueuedPathFromCache(true);
        }

        /// <summary>
        /// Sets the handler's path to use the cached queue path that has been generated. If there is no cache queue path that has
        /// been generated beforehand, the function will return. See <see cref="ComputeCachedDestinationCavernPath"/>.
        /// </summary>
        public void SetQueuedPathFromCache(bool setTunnelSteeringUntilQueuePathEnd = false)
        {
            if (cachedPointPath.IsNullOrEmpty())
            {
                if (enableDebug) $"There is no cached point path to run. Please compute the cached path before calling this.".Msg();
                return;
            }
            if (enableDebug) "Running cached queue path...".Msg();
            SetQueuedPath(cachedPointPath);
            if (setTunnelSteeringUntilQueuePathEnd)
            {
                lockSteeringBehaviour = true;
                //TunnelModeSteering();
                if(enableDebug) Debug.LogWarning("STEERINGTUNNEL");

            }


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
            canTimeout = true;
            canAutoSelectNavPoints = true;
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug)
            {
                string debugPath = "";
                foreach (NavPoint point in pointPath)
                    debugPath += point.Name + ",";

                $"Queued path set: {debugPath}".Msg();
            }
        }

        public void StopQueuedPath()
        {
            if (pointPath.IsNullOrEmpty() || !isOnQueuePath)
                return;

            pointPath.Clear();

            if (!chosenAmbushPoint)
            {
                canTimeout = true;
                canAutoSelectNavPoints = true;
            }

            isOnQueuePath = false;
            lockSteeringBehaviour = false;
            CavernModeSteering();
            currentPoint.Deselect();
            SkipCurrentPoint(true);

            if (enableDebug) "Stopping Queued path on request. Resuming normal movement.".Msg();
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
                if (isChasingAPlayer)
                {
                    isChasingAPlayer = false;
                    if (!isStunned) CavernModeSteering();
                    else StunnedModeSteering();

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

        /// <summary>
        /// Select a new ambush point to go
        /// </summary>
        public void SelectAmbushPoint()
        {
            if (cavernManager == null)
                return;

            if (currentPoint == null)
                currentPoint = GetClosestPointToSelf();

            List<NavPoint> ambushPoints = navPoints
                                          .Where(o => o != null && o != currentPoint && o.CavernTag == cavernManager.GetCavernTagOfAILocation() && o.IsHidingPoint)
                                          .ToList();

            ambushPoints.RemoveAll(p => p == null);
            if (ambushPoints.IsEmpty())
            {
                if (enableDebug)
                    $"Ambush points is empty; AI in {cavernManager.GetCavernTagOfAILocation()}".Msg();
                return;
            }

            if (currentPoint != null) currentPoint.Deselect();

            currentPoint = ambushPoints.RandomElement();
            currentPoint.Select();
            chosenAmbushPoint = true;
            hasReachedPoint = false;
            canTimeout = false;
            canAutoSelectNavPoints = false;
            pointPath.Clear();
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            if (enableDebug)
                $"Selected new point: {currentPoint.gameObject.name}; Brain current cavern: {cavernManager.GetCavernTagOfAILocation()}".Msg();
        }

        public void ResetAmbushPoint()
        {
            canTimeout = true;
            canAutoSelectNavPoints = true;
            chosenAmbushPoint = false;
            if (rBody != null) rBody.isKinematic = false;
            ResetNavPointLingerTimer();
            ResetTimeoutTimer();
            SkipCurrentPoint(true);
        }
        public void TunnelModeSteering()
        {
            _steeringMode = SteeringMode.Tunnel;
            DecideCurrentSteeringMode();

        }

        public void CavernModeSteering()
        {
            _steeringMode = SteeringMode.Cavern;
            DecideCurrentSteeringMode();
        }

        public void StunnedModeSteering()
        {
            _steeringMode = SteeringMode.Stunned;
            DecideCurrentSteeringMode();
        }

        public void EngageModeSteering()
        {
            _steeringMode = SteeringMode.Engage;
            DecideCurrentSteeringMode();
        }

        private void DecideCurrentSteeringMode()
        {
            if (lockSteeringBehaviour)
                return;

            if (currentSteer != null)
                currentSteer.UnsubscribeAllEvents();

            currentSteer = _steeringMode switch
            {
                SteeringMode.Cavern => cavernSteeringSettings,
                SteeringMode.Tunnel => tunnelSteeringSettings,
                SteeringMode.Stunned => stunnedSteeringSettings,
                SteeringMode.Engage => engagementSteeringSettings,
                _ => null
            };

            currentSteer.OnSettingsUpdate += UpdateSteering;

            UpdateSteering();
        }

        private void UpdateSteering()
        {
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

        /// <summary>
        /// Provides constant forward force on the pilot
        /// </summary>
        private void MoveForwards(in float deltaTime)
        {
            if (TotalMaxVelocity < float.Epsilon) return;
            float thrust = TotalThrustForce;
            float modifiedSpeed = thrust - (thrust * slowMultiplier);
            Vector3 force = pilotTrans.forward * (modifiedSpeed * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);
        }

        private void HandleSpeedAndDirection(in float deltaTime)
        {
            if (currentPoint != null && TotalMaxVelocity > float.Epsilon && !isStunned && isChasingAPlayer)
            {
                //! Chasing player direction
                Vector3 moveTo = (currentPoint.GetPosition - pilotTrans.position).normalized * TotalAttractionForce;
                rBody.velocity = Vector3.Lerp(rBody.velocity, rBody.velocity + moveTo, deltaTime * attractionForce);
            }

            //! Look at
            Vector3 lerpResult;
            float totalLerpSpeed = deltaTime * smoothLookAtSpeed;

            if (_lookAtTarget == null)
                lerpResult = Vector3.Lerp(pilotTrans.forward, rBody.velocity.normalized, totalLerpSpeed);
            else
                lerpResult = Vector3.RotateTowards(pilotTrans.forward, _lookAtTarget.position, totalLerpSpeed, 0f); //Vector3.Lerp(pilotTrans.forward, (_lookAtTarget.position - pilotTrans.position).normalized, totalLerpSpeed);

            pilotTrans.forward = lerpResult;
        }

        private void ClampMaxVelocity()
        {
            if (TotalMaxVelocity < float.Epsilon)
            {
                rBody.velocity = Vector3.zero;
                return;
            }

            if (rBody.velocity.magnitude > TotalMaxVelocity * (1f - slowMultiplier))
                rBody.velocity = rBody.velocity.normalized * TotalMaxVelocity;
        }

        private void MoveTowardsCurrentNavPoint(in float deltaTime)
        {
            if (currentPoint == null || TotalMaxVelocity < float.Epsilon) return;
            Vector3 direction = currentPoint.GetDirectionTo(pilotTrans.position);

            float attraction = TotalAttractionForce;
            float modifiedSpeed = attraction - (attraction * slowMultiplier);
            Vector3 force = direction * (modifiedSpeed * deltaTime);
            rBody.AddForce(force, ForceMode.VelocityChange);

            if (!hasReachedPoint && CloseEnoughToTargetNavPoint())
            {
                hasReachedPoint = true;
                OnReachedPoint?.Invoke();
                EvaluateReachedPoint();
                if (enableDebug) $"Point Reached: {currentPoint.gameObject.name}".Msg();
            }

            //! Local shorthands
            bool CloseEnoughToTargetNavPoint() => currentPoint.GetSqrDistanceTo(pilotTrans.position) < (closeNavPointDetectionRadius * closeNavPointDetectionRadius);

            void EvaluateReachedPoint()
            {
                //! Check if point path is not empty and return after evaluation if true
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

                //! As long as it is not an ambush point, canTimeout and Auto select nav points can be reset to true. 
                if (!chosenAmbushPoint)
                {
                    canTimeout = true;
                    canAutoSelectNavPoints = true;
                }
                else //! If it is an ambush point, make the pilot assume a static position and rotation in 
                {
                    if (rBody != null)
                    {
                        rBody.isKinematic = true;
                        rBody.velocity = Vector3.zero;
                    }
                    pilotTrans.position = currentPoint.GetPosition;
                    ChangeRotationOnAmbushPoints();

                }

                //! If the boolean is true, make it false & reset some variables (if this is called it means that the queue point path is empty)
                if (isOnQueuePath)
                {
                    isOnQueuePath = false;
                    lockSteeringBehaviour = false;
                    CavernModeSteering();
                    if (enableDebug) "Queued path is done.".Msg();
                }
            }
        }

        /// <summary>
        /// Check ambush point's rotation and align itself properly based on different ambush points.
        /// </summary>
        private void ChangeRotationOnAmbushPoints()
        {
            if (currentPoint.name == "Crystal_NavPoint_Ambush")
            {
                pilotTrans.rotation = Quaternion.Euler(currentPoint.transform.rotation.x + 85, currentPoint.transform.rotation.y + 140, currentPoint.transform.rotation.z + 220);
            }
            else
            {
                pilotTrans.rotation = Quaternion.RotateTowards(pilotTrans.rotation, currentPoint.transform.rotation, 180);
            }
        }

        private void HandleObstacleAvoidance(in float deltaTime)
        {
            obstacleCheckTimer -= deltaTime;
            if (!ObstacleTimerReached || TotalMaxVelocity < float.Epsilon) return;
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
                Vector3 forceNormalised = force.normalized;
                if (!isChasingAPlayer)
                {
                    Vector3 cross = Vector3.Cross(forceNormalised, rBody.velocity.normalized);
                    if (cross.magnitude.Abs() <= 0.2f)
                    {
                        Vector3 direction = forceNormalised;

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
            if (!_tickCavernLingerTimer || !cavernManager) return;

			if (!ignoreCavernLingerTimer)
			{
				switch (cavernManager.GetCavernTagOfAILocation())
				{
					case CavernTag.Crystal:
						{
							crystalCavernLingerTimer -= deltaTime;
							if (enableDebug) Debug.Log("crystal: " + crystalCavernLingerTimer);
							if (crystalCavernLingerTimer > 0f) return;
							break;
						}
					case CavernTag.Bioluminescent:
						{
							biolumiCavernLingerTimer -= deltaTime;
							if (enableDebug) Debug.Log("biolumi: " + biolumiCavernLingerTimer);
							if (biolumiCavernLingerTimer > 0f) return;
							break;
						}
					case CavernTag.Hydrothermal_Deep:
						{
							hydrothermalCavernLingerTimer -= deltaTime;
							if (enableDebug) Debug.Log("hydrothermal: " + hydrothermalCavernLingerTimer);
							if (hydrothermalCavernLingerTimer > 0f) return;
							break;
						}
					case CavernTag.Lair:
						{
							lairCavernLingerTimer -= deltaTime;
							if (enableDebug) Debug.Log("lair: " + lairCavernLingerTimer);
							if (lairCavernLingerTimer > 0f) return;
							break;
						}
					default: return;
				}
			}

            _tickCavernLingerTimer = false;
            ResetCavernLingerTimer();
            SetQueuedPathFromCache(true);
        }

        /// <summary> The actual logic that selects a new nav point. The algorithm behaviour can be adjusted with
        /// the <see cref="numberOfClosestPointsToConsider"/> variable. </summary>
        private void SelectNewNavPoint()
        {
            if (cavernManager == null)
                return;

            if (currentPoint == null)
                currentPoint = GetClosestPointToSelf();

            CavernTag aiTag = cavernManager.GetCavernTagOfAILocation();
            if (aiTag == CavernTag.Invalid)
                aiTag = Data_CachedLatestCavernTag;

            List<NavPoint> potentialPoints = navPoints
                                            .Where(o => o != null
                                                    && o != currentPoint
                                                    && o.CavernTag == aiTag
                                                    && !o.IsTunnelEntry
                                                    && !o.IsHidingPoint)
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
        private void ResetCavernLingerTimer()
        {
            crystalCavernLingerTimer = GetNextCrystalCavernLingerTime();
            biolumiCavernLingerTimer = GetNextBiolumiCavernLingerTime();
            lairCavernLingerTimer = GetNextLairCavernLingerTime();
            hydrothermalCavernLingerTimer = GetNextHydrothermalCavernLingerTime();
        }

        private float GetNextNavPointLingerTime() => Random.Range(navPointLingerTimeRange.x, navPointLingerTimeRange.y);
        private float GetNextCrystalCavernLingerTime() => Random.Range(crystalCavernLingerTimeRange.x, crystalCavernLingerTimeRange.y);
        private float GetNextBiolumiCavernLingerTime() => Random.Range(biolumiCavernLingerTimeRange.x, biolumiCavernLingerTimeRange.y);
        private float GetNextLairCavernLingerTime() => Random.Range(lairCavernLingerTimeRange.x, lairCavernLingerTimeRange.y);
        private float GetNextHydrothermalCavernLingerTime() => Random.Range(hydrothermalCavernLingerTimeRange.x, hydrothermalCavernLingerTimeRange.y);


        public void SetAIStunned(bool isStun) => isStunned = isStun;

        #endregion

        #region Shorthands

        private float BaseVelocityMultiplier => maxVelocity * debugVelocityMultiplier;
        private float SlowedVelocityModifier => BaseVelocityMultiplier * slowMultiplier;
        private float SpedUpVelocityModifier => BaseVelocityMultiplier * speedMultiplier;
        public float TotalMaxVelocity => BaseVelocityMultiplier - SlowedVelocityModifier + SpedUpVelocityModifier;
        public bool CanMove => _isEnabled && pilotTrans != null && rBody != null && PhotonNetwork.IsMasterClient;
        public bool HasPlayerTarget => isChasingAPlayer && isOnCustomPath;
        public Rigidbody Rigidbody => rBody;

        #endregion
    }
}
