using Hadal.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Inputs;
using Hadal.AudioSystem;

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
        ForwardLeft,
        ForwardRight,
        BackwardLeft,
        BackwarRight,
        None
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
        [SerializeField] private AudioEventData boostSound;

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
                    if(boostSound)
                        boostSound.PlayOneShot(playerController.GetTarget);

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

                    case BoostDirection.ForwardLeft:
                        playerController.GetInfo.Rigidbody.AddRelativeForce((Vector3.forward + Vector3.left) / 2 * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.ForwardRight:
                        playerController.GetInfo.Rigidbody.AddRelativeForce((Vector3.forward + Vector3.right) / 2 * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.BackwardLeft:
                        playerController.GetInfo.Rigidbody.AddRelativeForce((Vector3.back + Vector3.left) / 2 * dodgeForce, ForceMode.Acceleration);
                        return;

                    case BoostDirection.BackwarRight:
                        playerController.GetInfo.Rigidbody.AddRelativeForce((Vector3.back + Vector3.right) / 2 * dodgeForce, ForceMode.Acceleration);
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

            boostDirection = BoostDirection.Forward;

            if (input.VerticalForward && input.HorizontalRight)
            {
                boostDirection = BoostDirection.ForwardRight;
            }
            else if (input.VerticalForward && input.HorizontalLeft)
            {
                boostDirection = BoostDirection.ForwardLeft;
            }
            else if (input.VerticalBackward && input.HorizontalRight)
            {
                boostDirection = BoostDirection.BackwarRight;
            }
            else if (input.VerticalBackward && input.HorizontalLeft)
            {
                boostDirection = BoostDirection.BackwardLeft;
            }
            else if (input.VerticalForward)
            {
                boostDirection = BoostDirection.Forward;
            }
            else if (input.HorizontalRight)
            {
                boostDirection = BoostDirection.Right;
            }
            else if (input.HorizontalLeft)
            {
                boostDirection = BoostDirection.Left;
            }
            else if (input.HoverUp)
            {
                boostDirection = BoostDirection.Up;
            }
            else if (input.HoverDown)
            {
                boostDirection = BoostDirection.Down;
            }
            else if (input.VerticalBackward)
            {
                boostDirection = BoostDirection.Backward;
            }

            isBoosting = input.BoostActive;
            return isBoosting;
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
            UpdateReserveCount(maxReserveCapacity);
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
