using NaughtyAttributes;
using System;
using UnityEngine;
using Hadal.Utility;
using Hadal.InteractableEvents;
using Hadal.AudioSystem;

//Created by Jet
namespace Hadal.Usables
{
    public class UsableLauncherObject : MonoBehaviour, IUsable, IUnityServicer
    {
		[Header("General")]
        [SerializeField] private UsableLauncherData data;
        public virtual UsableLauncherData Data { get => data; set => data = value; }

        /// <summary> For display usage of utility name. </summary>
        public string UtilityName;

        /// <summary> For display if the launcher is Powered. </summary>
        public bool IsPowered;
        /// <summary> Event is called when <see cref="Use"/> is called succesfully. Can be used to cue sfx or animations. </summary>
        public virtual event Action<UsableLauncherObject> OnFire;

        /// <summary> Event is called when any ammunition/charges are restored (if any). </summary>
        public event Action<UsableLauncherObject> OnRestock;

        /// <summary> Event is called everytime when the usable object is equipped or dequipped. 
        /// <br/>
        /// The boolean returns true if the usable object has been <strong>equipped</strong>. </summary>
        public event Action<UsableLauncherObject, bool> OnSwitch;
        public bool HasToggleAmmo { get; private set; } = false;
        public bool isEquipLocked { get; set; } = false;
        public bool IgnoreAmmo { get; set; } = false;

        // public float ChargedTime;
        protected Camera PCamera { get; set; } = null;
        protected bool IsActive { get; set; } = false;

        #region Utility Reloading Logic Variables
        [SerializeField] protected int maxReserveCapacity;
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
        
        [Header("Audio")]
        [SerializeField] private AudioEventData chamberReloadedAudio;
        [SerializeField] private AudioEventData reverseReloadedAudio;
        #endregion

        #region Unity Lifecycle

        protected virtual void Awake() 
        {
            SetDefaults();
            BuildTimers();
            IsActive = true;
        }

        private void FixedUpdate() => DoFixedUpdate(Time.fixedDeltaTime);
        #endregion

        #region Command Methods

        public void Inject(Camera camera) => PCamera = camera;
        public void Activate()
        {
            IsActive = true;
            OnSwitch?.Invoke(this, IsActive);
        }
        public void Deactivate()
        {
            IsActive = false;
            OnSwitch?.Invoke(this, IsActive);
        }
        public virtual void ToggleAmmo()
        {
            Data.ToggleProjectile(HasToggleAmmo);
        }

        public virtual void PowerUp()
        {
            return;
        }

        public virtual void ReceiveInteractEvent(InteractionType interactionType, int interactableID, int reloadAmount)
        {
            return;
        }

        #endregion

        #region Use Method

        /// <summary> Method that does what the usable launcher object should do. Can be overriden by subclasses for custom behaviour. </summary>
        /// <param name="info">Information that any usable launcher object may need to perform its task.</param>
        public virtual bool Use(UsableHandlerInfo info)
        {
            if (!IsActive) return false;
            OnFire?.Invoke(this);
            LaunchToDestination(info);
            return true;
        }

        /// <summary> Calls <see cref="UsableLauncherObject.Data"/>'s DoEffect. </summary>
        protected virtual void LaunchToDestination(UsableHandlerInfo info)
            => Data.DoEffect(info);

        #endregion

        #region Utility Reload Methods
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

            if(chamberReloadedAudio)
            {
                chamberReloadedAudio.PlayOneShot2D();
            }
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

            if (reverseReloadedAudio)
            {
                reverseReloadedAudio.PlayOneShot2D();
            }
        }

        public void IncrementReserve(int reloadAmount)
        {
            IsRegenerating = false;
            UpdateReserveCount(ReserveCount + reloadAmount);
            OnReservesChanged?.Invoke(true);

            if (reverseReloadedAudio)
            {
                reverseReloadedAudio.PlayOneShot2D();
            }
        }

        public void ChangeChamberReloadTime(float newReloadTime)
        {
            chamberReloadTime = newReloadTime;
            _chamberReloadTimer.RestartWithDuration(chamberReloadTime);
            _chamberReloadTimer.CompletedOnStart();
        }
		
		public void ChangeReserveRegenTime(float newRegenTime)
		{
			reserveRegenerationTime = newRegenTime;
			_reserveRegenTimer.RestartWithDuration(reserveRegenerationTime);
			_reserveRegenTimer.Pause();
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
        public void SetDefaults()
        {
            UpdateReserveCount(maxReserveCapacity);
            if (maxOnLoadOut) UpdateChamberCount(maxChamberCapacity);
            IsRegenerating = false;
            IsReloading = false;
        }
        #endregion

        #region Interface Implementations

        public void OnRestockInvoke() => OnRestock?.Invoke(this);
        public virtual void DoUpdate(in float deltaTime) 
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
        public virtual void DoFixedUpdate(in float fixedDeltaTime) { }
        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;

        #endregion

        #region Accessors

        public int AssociatedProjectileTypeID => data.ProjectileData.ProjTypeInt;

        #endregion
    }
}
