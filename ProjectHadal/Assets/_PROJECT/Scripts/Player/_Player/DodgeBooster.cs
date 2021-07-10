using Hadal.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Inputs;

namespace Hadal.Player
{
    public class DodgeBooster : MonoBehaviour, IPlayerComponent
    {
        public event Action<DodgeBooster> OnRestock;
        private IMovementInput input;
        private PlayerController playerController;

        [SerializeField, Range(0.1f, 0.6f)] private float tapDetectionTime;
        [SerializeField] private float dodgeForce;

        #region Boost Refill logic
        [SerializeField] private int maxReserveCapacity;
        [SerializeField] private float reserveRegenerationTime;

        public int ReserveCount { get; private set; }
        public bool IsRegenerating { get; private set; }
        public float ReserveRegenRatio => (_reserveRegenTimer.IsCompleted) ? 0f : _reserveRegenTimer.GetCompletionRatio;
        public bool HasAnyReserves => ReserveCount > 0;
        public event Action<bool> OnReservesChanged;
        private Timer _reserveRegenTimer;

        [SerializeField] protected int maxChamberCapacity;
        [SerializeField] private float chamberReloadTime;
        [SerializeField] private bool maxOnLoadOut = true;
        public int ChamberCount { get; private set; }
        public bool IsReloading { get; set; }
        public float ChamberReloadRatio => (_chamberReloadTimer.IsCompleted && TotalAmmoCount == 0) ? 0f : _chamberReloadTimer.GetCompletionRatio;
        public bool IsChamberLoaded => ChamberCount > 0;
        public event Action<bool> OnChamberChanged;
        protected Timer _chamberReloadTimer;

        public int TotalAmmoCount => ReserveCount + ChamberCount;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetDefaults();
            BuildTimers();
            input = new RawMovementInput();
            input.DoubleTapDetectionTime = tapDetectionTime;
        }

        public void DoUpdate(float deltaTime)
        {
            if(IsChamberLoaded)
            {
                if(CheckForInput())
                {
                    DecrementChamber();
                }
            }

            if (ReserveCount < maxReserveCapacity && !IsRegenerating)
            {
                IsRegenerating = true;
                _reserveRegenTimer.Restart();
            }

            if (ChamberCount < maxChamberCapacity && !IsReloading && HasAnyReserves)
            {
                IsReloading = true;
                _chamberReloadTimer.Restart();
            }
        }
        #endregion

        public void Inject(PlayerController controller)
        {
            playerController = controller;
        }

        private bool CheckForInput()
        {
            bool result = true;
            if (input.DoubleHorizontalRight)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.right * dodgeForce, ForceMode.Impulse);
            }
            else if (input.DoubleHorizontalLeft)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.left * dodgeForce, ForceMode.Impulse);
            }
            else if (input.DoubleHoverUp)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.up * dodgeForce, ForceMode.Impulse);
            }
            else if (input.DoubleHoverDown)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.down * dodgeForce, ForceMode.Impulse);
            }
            else if (input.DoubleVerticalForward)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.forward * dodgeForce, ForceMode.Impulse);
            }
            else if (input.DoubleVerticalBackward)
            {
                playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.back * dodgeForce, ForceMode.Impulse);
            }
            else
            {
                result = false;
            }

            return result;
        }

        #region Booster Reload Methods
        public void DecrementChamber()
        {
            UpdateChamberCount(ChamberCount - 1);
            OnChamberChanged?.Invoke(false);
        }
        public void IncrementChamber()
        {
            IsReloading = false;
            DecrementReserve();
            UpdateChamberCount(ChamberCount + 1);
            OnChamberChanged?.Invoke(true);
            OnRestockInvoke();
        }
        private void DecrementReserve()
        {
            UpdateReserveCount(ReserveCount - 1);
            OnReservesChanged?.Invoke(false);
        }
        public void IncrementReserve()
        {
            IsRegenerating = false;
            UpdateReserveCount(ReserveCount + 1);
            OnReservesChanged?.Invoke(true);
        }

        private void UpdateReserveCount(in int count) => ReserveCount = Mathf.Clamp(count, 0, maxReserveCapacity);
        private void UpdateChamberCount(in int count) => ChamberCount = Mathf.Clamp(count, 0, maxChamberCapacity);

        public void OnRestockInvoke() => OnRestock?.Invoke(this);
        private void BuildTimers()
        {
            _reserveRegenTimer = this.Create_A_Timer()
                                .WithDuration(reserveRegenerationTime)
                                .WithOnCompleteEvent(IncrementReserve)
                                .WithShouldPersist(true);
            _chamberReloadTimer = this.Create_A_Timer()
                                .WithDuration(chamberReloadTime)
                                .WithOnCompleteEvent(IncrementChamber)
                                .WithShouldPersist(true);
            _reserveRegenTimer.Pause();
            _chamberReloadTimer.CompletedOnStart();
        }
        private void SetDefaults()
        {
            UpdateReserveCount(maxReserveCapacity);
            if (maxOnLoadOut) UpdateChamberCount(maxChamberCapacity);
            IsRegenerating = false;
            IsReloading = false;
        }
        #endregion
    }
}
