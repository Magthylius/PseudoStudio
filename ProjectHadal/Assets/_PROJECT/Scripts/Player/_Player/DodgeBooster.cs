using Hadal.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Inputs;

namespace Hadal.Player
{
    enum BoostDirection
    {
        Left = 0,
        Right,
        Up,
        Down,
        Forward,
        Backward,
    };
    public class DodgeBooster : MonoBehaviour, IPlayerComponent
    {
        public event Action<DodgeBooster> OnRestock;
        private IMovementInput input;
        private PlayerController playerController;

        [SerializeField, Range(0.1f, 0.6f)] private float tapDetectionTime;
        [SerializeField] private float dodgeForce;
        private bool isBoosting;
        private float boostTimer;
        private BoostDirection boostDirection;
        [SerializeField] private float boostTimerMax;

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

            if (isBoosting)
            {
                boostTimer += deltaTime;

                if (boostTimer > boostTimerMax)
                {
                    isBoosting = false;
                    playerController.GetInfo.Rigidbody.velocity /= 2;
                    boostTimer = 0;
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

        public void DoFixedUpdate(float fixedDeltaTime)
        {
            if(isBoosting)
            {
                switch (boostDirection) 
                {
                    case BoostDirection.Left:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.left * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.Right:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.right * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.Up:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.up * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.Down:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.down * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.Forward:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.forward * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.Backward:
                        playerController.GetInfo.Rigidbody.AddRelativeForce(Vector3.back * dodgeForce, ForceMode.Acceleration);
                        return;

                }
            }
        }
        #endregion

        public void Inject(PlayerController controller)
        {
            playerController = controller;
        }

        private bool CheckForInput()
        {
            if (isBoosting)
                return false;

            bool result = true;

            if (input.DoubleHorizontalRight)
            {
                boostDirection = BoostDirection.Right;
            }
            else if (input.DoubleHorizontalLeft)
            {
                boostDirection = BoostDirection.Left;
            }
            else if (input.DoubleHoverUp)
            {
                boostDirection = BoostDirection.Up;
            }
            else if (input.DoubleHoverDown)
            {
                boostDirection = BoostDirection.Down;
            }
            else if (input.DoubleVerticalForward)
            {
                boostDirection = BoostDirection.Forward;
            }
            else if (input.DoubleVerticalBackward)
            {
                boostDirection = BoostDirection.Backward;
            }
            else
            {
                result = false;
            }

            isBoosting = result;
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

        public void ChangeMaxReserveCount(int newMaxReserve)
        {
            maxReserveCapacity = newMaxReserve;
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
