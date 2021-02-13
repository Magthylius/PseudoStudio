//created by Jin, edited by Jon, edited by Jey
using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;
using static Hadal.FluentBool;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviourDebug
    {
        [SerializeField] string debugKey;

        [Header("Torpedo")]
        [SerializeField] TorpedoLauncherObject tLauncher;
        [SerializeField] Transform torpedoFirePoint;
        [SerializeField] float torpedoForce;

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
        }

        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
        }

        public void DoUpdate(in float deltaTime)
        {
            OnUnityUpdateUI();
        }

        #endregion

        #region Handler Methods

        public void FireTorpedo()
        {
            if (!tLauncher.IsChamberLoaded) return;
            HandleTorpedoObject();
        }

        public void FireUtility(UsableObject usable)
        {
            if (!_canUtilityFire) return;
            HandleUtilityReloadTimer();
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
            _utilityReloadTimer = this.Create_A_Timer()
                        .WithDuration(utilityFireDelay)
                        .WithOnCompleteEvent(SetCanUtilityFire)
                        .WithShouldPersist(true);
            _utilityReloadTimer.Pause();
        }
        private void HandleUtilityReloadTimer()
        {
            _canUtilityFire = false;
            _utilityReloadTimer.Restart();
        }
        private void SetCanUtilityFire() => _canUtilityFire = true;

        #endregion
    }
}