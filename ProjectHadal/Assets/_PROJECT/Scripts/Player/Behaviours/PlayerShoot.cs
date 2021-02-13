//created by Jin, edited by Jon
//edited by Jey
using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] TorpedoLauncherObject tLauncher;
        [SerializeField] Transform firePoint;
        [SerializeField] float fireDelay;
        [SerializeField] float force;
        private Timer _reloadTimer;
        private bool _canFire;

        #region Unity Lifecycle

        private void Awake()
        {
            _canFire = true;
            BuildTimer();
        }

        private void Start()
        {
            UpdateUIChamberCount();
            tLauncher.OnChamberChanged += OnChamberChangedMethod;
        }

        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
        }

        public void DoUpdate(in float deltaTime)
        {
            OnUnityUpdateUI();
        }

        #endregion

        #region Handler Methods

        public void Fire(UsableObject usable)
        {
            if (!_canFire) return;
            HandleFireTimer();
            HandleUsable(usable);
        }

        private void HandleUsable(UsableObject usable)
        {
            if (usable is TorpedoLauncherObject torpedo)
            {
                if (!torpedo.IsChamberLoaded) return;
                torpedo.DecrementChamber();
                torpedo.Use(CreateInfo());
                return;
            }
            usable.Use(CreateInfo());
        }

        private UsableHandlerInfo CreateInfo() => new UsableHandlerInfo().WithTransformInfo(firePoint).WithForce(force);
        
        #endregion

        #region UI

        private void OnChamberChangedMethod(bool isIncrement)
        {
            UpdateUIChamberCount();
            if (isIncrement) UpdateUIFloodRatio(1f);
        }
        private void OnUnityUpdateUI()
        {
            if (tLauncher.IsReloading)
                UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
        }
        private void UpdateUIChamberCount()
        {
            UIManager.Instance
            .UpdateTubes(tLauncher.TotalTorpedoes);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            UIManager.Instance
            .UpdateFlooding(ratio.Clamp01());
        }

        #endregion

        #region Timer

        private void BuildTimer()
        {
            _reloadTimer = this.Create_A_Timer()
                        .WithDuration(fireDelay)
                        .WithOnCompleteEvent(SetCanFire)
                        .WithShouldPersist(true);
            _reloadTimer.Pause();
        }
        private void HandleFireTimer()
        {
            _canFire = false;
            _reloadTimer.Restart();
        }
        private void SetCanFire() => _canFire = true;

        #endregion
    }
}