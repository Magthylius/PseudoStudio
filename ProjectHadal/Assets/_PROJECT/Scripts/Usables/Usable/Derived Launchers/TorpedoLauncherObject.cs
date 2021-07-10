using NaughtyAttributes;
using Hadal.Utility;
using Hadal.InteractableEvents;
using UnityEngine;
using System;

//Created by Jey
namespace Hadal.Usables
{
    public class TorpedoLauncherObject : UsableLauncherObject
    {
        public override void DoUpdate(in float deltaTime)
        {
            if (ChamberCount < maxChamberCapacity && !IsReloading && HasAnyReserves)
            {
                IsReloading = true;
                _chamberReloadTimer.Restart();
            }
        }

        private void OnDisable()
        {
            InteractableEventManager.Instance.OnInteraction -= ReceiveInteractEvent;
        }

        public void SubscribeToSalvageEvent()
        {
            InteractableEventManager.Instance.OnInteraction += ReceiveInteractEvent;
        }

        public override void ReceiveInteractEvent(InteractionType interactionType)
        {
            if(interactionType == InteractionType.Salvage_Torpedo)
            {
                IncrementReserve();
            }
        }
            /*  private const string ReserveGroupName = "Reserves";
              private const string ChamberGroupName = "Chamber";

              #region Variable Definitions

              [Foldout(ReserveGroupName), SerializeField] private int maxReserveCapacity;
              [Foldout(ReserveGroupName), SerializeField] private float reserveRegenerationTime;
              public int ReserveCount { get; private set; }
              public bool IsRegenerating { get; private set; }
              public float ReserveRegenRatio => (_reserveRegenTimer.IsCompleted) ? 0f : _reserveRegenTimer.GetCompletionRatio;
              public bool HasAnyReserves => ReserveCount > 0;
              public event Action<bool> OnReservesChanged;
              private Timer _reserveRegenTimer;

              [Foldout(ChamberGroupName), SerializeField] private int maxChamberCapacity;
              [Foldout(ChamberGroupName), SerializeField] private float chamberReloadTime;
              [Foldout(ChamberGroupName), SerializeField] private bool maxOnLoadOut = true;
              public int ChamberCount { get; private set; }
              public bool IsReloading { get; private set; }
              public float ChamberReloadRatio => (_chamberReloadTimer.IsCompleted && TotalTorpedoes == 0) ? 0f : _chamberReloadTimer.GetCompletionRatio;
              public bool IsChamberLoaded => ChamberCount > 0;
              public event Action<bool> OnChamberChanged;
              private Timer _chamberReloadTimer;

              public int TotalTorpedoes => ReserveCount + ChamberCount;

              #endregion

              protected override void Awake()
              {
                  SetDefaults();
                  BuildTimers();
                  IsActive = true;
              }

              public override void DoUpdate(in float deltaTime)
              {
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

              public void DecrementChamber()
              {
                  UpdateChamberCount(ChamberCount - 1);
                  OnChamberChanged?.Invoke(false);
              }
              private void IncrementChamber()
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
              private void IncrementReserve()
              {
                  IsRegenerating = false;
                  UpdateReserveCount(ReserveCount + 1);
                  OnReservesChanged?.Invoke(true);
              }

              private void UpdateReserveCount(in int count) => ReserveCount = Mathf.Clamp(count, 0, maxReserveCapacity);
              private void UpdateChamberCount(in int count) => ChamberCount = Mathf.Clamp(count, 0, maxChamberCapacity);

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
              }*/
    }
}