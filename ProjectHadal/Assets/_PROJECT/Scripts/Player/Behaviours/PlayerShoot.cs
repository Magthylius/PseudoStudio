//created by Jin, edited by Jon, edited by Jey
using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;
using static Hadal.FluentBool;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviourDebug, IPlayerComponent
    {
        [SerializeField] string debugKey;

        [Header("Aiming")]
        public Transform aimParentObject;
        public Transform aimPoint;

        float aimPointYDelta;
        Ray aimingRay;

        [Header("Torpedo")]
        [SerializeField] TorpedoLauncherObject tLauncher;
        [SerializeField] Transform torpedoFirePoint;
        [SerializeField] float torpedoForce;
        public TorpedoLauncherObject GetTorpedoLauncher => tLauncher;

        [Header("Utility")]
        [SerializeField] Transform utilityFirePoint;
        [SerializeField] float utilityFireDelay;
        [SerializeField] float utilityForce;
        private Timer _utilityReloadTimer;
        private bool _canUtilityFire;
        
        #region Unity Lifecycle

        private void Awake()
        {
            BuildTimers();
            tLauncher.OnChamberChanged += OnChamberChangedMethod;
            tLauncher.OnReservesChanged += OnReserveChangedMethod;
        }

        private void Start()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            DoDebugEnabling(debugKey);

            aimingRay = new Ray(aimPoint.position, aimParentObject.forward * 1000f);
            aimPointYDelta = aimParentObject.position.y - aimPoint.position.y;
        }

        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
        }

        void OnDrawGizmos()
        {

            Gizmos.DrawRay(aimingRay);
            Gizmos.DrawLine(aimPoint.position, aimParentObject.forward * 1000f);
        }

        public void DoUpdate(in float deltaTime)
        {
            OnUnityUpdateUI();
            CalculateTorpedoAngle();
        }

        #endregion

        #region Handler Methods
        public void CalculateTorpedoAngle()
        {
            RaycastHit aimHit;
            if (Physics.Raycast(aimingRay, out aimHit, Mathf.Infinity))
            {
                float angle = Mathf.Atan2(aimHit.point.y - aimPoint.position.y, aimHit.point.x - aimPoint.position.x);
                DebugLog(angle);
                return;
            }

            DebugLog("No angle");
        }


        public void FireTorpedo()
        {
            if (!tLauncher.IsChamberLoaded) return;
            HandleTorpedoObject();
        }

        public void FireUtility(UsableObject usable)
        {
            if (!_canUtilityFire) return;
            HandleUtilityReloadTimer(usable);
            usable.Use(CreateInfoForUtility());
        }

        private void HandleTorpedoObject()
        {
            tLauncher.DecrementChamber();
            tLauncher.Use(CreateInfoForTorpedo());
        }

        private UsableHandlerInfo CreateInfoForTorpedo() => new UsableHandlerInfo().WithTransformInfo(torpedoFirePoint).WithForce(torpedoForce);
        private UsableHandlerInfo CreateInfoForUtility() => new UsableHandlerInfo().WithTransformInfo(utilityFirePoint).WithForce(utilityForce);

        #endregion

        #region UI

        private void OnChamberChangedMethod(bool isIncrement)
        {
            UpdateUITorpedoCount();
            if (isIncrement)
            {
                UpdateUIFloodRatio(1f);
                DebugLog("Torpedo Flooded!");
                return;
            }

            if (tLauncher.ChamberCount == 0)
            {
                UpdateUIFloodRatio(0f);
            }
            DebugLog("Torpedo Fired!");
        }
        private void OnReserveChangedMethod(bool isIncrement)
        {
            UpdateUITorpedoCount();
            if (Not(isIncrement)) return;
            DebugLog("Torpedo Regenerated (Loaded)!");
        }
        private void OnUnityUpdateUI()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            UpdateUIRegenRatio(tLauncher.ReserveRegenRatio);
        }
        private void UpdateUITorpedoCount()
        {
            UIManager.Instance
            .UpdateTubes(tLauncher.TotalTorpedoes);
        }
        private void UpdateUIRegenRatio(in float ratio)
        {
            UIManager.Instance
            .UpdateReload(ratio.Clamp01(), tLauncher.IsRegenerating);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            UIManager.Instance
            .UpdateFlooding(ratio.Clamp01(), tLauncher.IsReloading);
        }

        #endregion

        #region Timer

        private void BuildTimers()
        {
            SetCanUtilityFire();
            _utilityReloadTimer = this.Create_A_Timer()
                        .WithDuration(utilityFireDelay)
                        .WithOnCompleteEvent(SetCanUtilityFire)
                        .WithShouldPersist(true);
            _utilityReloadTimer.PausedOnStart();
        }
        private void HandleUtilityReloadTimer(UsableObject usable)
        {
            _canUtilityFire = false;
            _utilityReloadTimer.Restart();
            usable.OnRestockInvoke();
        }
        private void SetCanUtilityFire() => _canUtilityFire = true;

        public void Inject(PlayerController controller) { }

        #endregion
    }
}