using NaughtyAttributes;
using Hadal.Utility;
using UnityEngine;

//Created by Jey
namespace Hadal.Usables
{
    public class TorpedoLauncherObject : UsableObject
    {
        private const string ReserveGroupName = "Reserves";
        private const string ChamberGroupName = "Chamber";
        
        #region Variable Definitions

        [Foldout(ReserveGroupName), SerializeField] private int maxReserveCapacity;
        [Foldout(ReserveGroupName), SerializeField] private float reserveRegenerationTime;
        private Timer _reserveRegenTimer;
        public int ReserveCount { get; private set; }

        [Foldout(ChamberGroupName), SerializeField] private int maxChamberCapacity;
        [Foldout(ChamberGroupName), SerializeField] private float chamberReloadTime;
        [Foldout(ChamberGroupName), SerializeField] private bool maxOnLoadOut = true;
        private Timer _chamberReloadTimer;
        public int ChamberCount { get; private set; }

        private bool _isReloading;
        private bool _isRegenerating;
        private bool HasAnyReserves => ReserveCount > 0;

        #endregion

        protected override void Awake()
        {
            SetDefaults();
            BuildTimers();
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (ReserveCount < maxReserveCapacity && !_isRegenerating)
            {
                _isRegenerating = true;
                _reserveRegenTimer.Restart();
            }

            if (ChamberCount < maxChamberCapacity && !_isReloading && HasAnyReserves)
            {
                _isReloading = true;
                _chamberReloadTimer.Restart();
            }
        }

        public bool IsChamberLoaded => ChamberCount > 0;
        public void UnloadChamber() => UpdateChamberCount(ChamberCount - 1);
        private void ReloadChamber()
        {
            _isReloading = false;
            DepleteReserve();
            UpdateChamberCount(ChamberCount + 1);
        }
        private void DepleteReserve() => UpdateReserveCount(ReserveCount - 1);
        private void RenegerateReserve()
        {
            _isRegenerating = false;
            UpdateReserveCount(ReserveCount + 1);
        }

        private void UpdateReserveCount(in int count) => ReserveCount = Mathf.Clamp(count, 0, maxReserveCapacity);
        private void UpdateChamberCount(in int count) => ChamberCount = Mathf.Clamp(count, 0, maxChamberCapacity);
        
        private void BuildTimers()
        {
            _reserveRegenTimer = this.Create_A_Timer()
                                .WithDuration(reserveRegenerationTime)
                                .WithOnCompleteEvent(RenegerateReserve)
                                .WithShouldPersist(true);
            _chamberReloadTimer = this.Create_A_Timer()
                                .WithDuration(chamberReloadTime)
                                .WithOnCompleteEvent(ReloadChamber)
                                .WithShouldPersist(true);
            _reserveRegenTimer.Pause();
            _chamberReloadTimer.Pause();
        }
        private void SetDefaults()
        {
            UpdateReserveCount(maxReserveCapacity);
            if (maxOnLoadOut) UpdateChamberCount(maxChamberCapacity);
            _isRegenerating = false;
            _isReloading = false;
        }
    }
}