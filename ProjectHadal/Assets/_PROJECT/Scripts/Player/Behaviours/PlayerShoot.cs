//created by Jin, edited by Jon, edited by Jey

using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hadal.UI;
using Hadal.Networking;
using UnityEngine.Events;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviourDebug, IPlayerComponent, IPlayerEnabler
    {
        [SerializeField] string debugKey;

        NetworkEventManager neManager;

        [Header("Audio")] 
        public UnityEvent torpedoFireAudioEvent;
        
        [Header("Player")]
        [SerializeField] PlayerController controller;
		private bool _allowUpdate;
		private bool _canFire;

        [Header("Aiming")]
        public Rigidbody aimParentRb;
        [SerializeField] Transform firePoint;

        [Header("Launchers")]
        [SerializeField] private TorpedoLauncherObject tLauncher;
        [SerializeField] private HarpoonLauncherObject hLauncher;
        public TorpedoLauncherObject GetTorpedoLauncher => tLauncher;

        [Header("Event")]
        private PhotonView _pView;

        #region Unity Lifecycle
        private void OnEnable()
        {
            neManager = NetworkEventManager.Instance;
            neManager.AddListener(ByteEvents.PLAYER_TORPEDO_LAUNCH, REFireTorpedo);
        }
        
        private void Awake()
        {
			_allowUpdate = true;
			_canFire = true;
            BuildTimers();
            tLauncher.OnChamberChanged += OnChamberChangedMethod;
            tLauncher.OnReservesChanged += OnReserveChangedMethod;
            tLauncher.OnRestock += OnRestock;
        }

        private void Start()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            DoDebugEnabling(debugKey);

            // listen to salvage event, if local.
            if (NetworkEventManager.Instance.isOfflineMode)
            {
                if (controller == LocalPlayerData.PlayerController)
                {
                    tLauncher.SubscribeToSalvageEvent();
                    controller.UI.Initialize(tLauncher.TotalAmmoCount, hLauncher.TotalAmmoCount);
                    
                }
            }
            else
            {
               // need to only subscribe if local
                tLauncher.SubscribeToSalvageEvent();
                controller.UI.Initialize(tLauncher.TotalAmmoCount, hLauncher.TotalAmmoCount);
            }
        }
        
        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
            tLauncher.OnRestock -= OnRestock;
        }
        

        public void DoUpdate(in float deltaTime)
        {
            if (!AllowUpdate) return;
            OnUnityUpdateUI();
            tLauncher.DoUpdate(deltaTime);
        }

        #endregion

        #region Handler Methods

        public void StartShootTracer()
        {
            controller.UI.ShootTracer.Activate();
        }

        public void StopShootTracer()
        {
            controller.UI.ShootTracer.Deactivate();
        }
        
        public UsableHandlerInfo CalculateDeployAngle(UsableHandlerInfo info)
        {
            UIShootTracer tracer = controller.UI.ShootTracer;
            if (tracer == null || Vector3.Distance(tracer.HitPoint, transform.position) < 10f)
            {
                info.AimedPoint = firePoint.position + firePoint.forward;
            }
            else
            {
                info.AimedPoint = tracer.HitPoint;
            }
            return info;
        }

        //Fire torpedo when received event
        private void REFireTorpedo(EventData obj)
        {
            if (obj.Code == (byte)ByteEvents.PLAYER_TORPEDO_LAUNCH)
            {
                object[] data = (object[])obj.CustomData;

                if ((int)data[0] == _pView.ViewID)
                {
                    FireTorpedo((int)data[1], true, (Vector3)data[2]);
                }
            }
        }

        //! Event Firing
        public void SendTorpedoEvent(int projectileID, Vector3 lookAtPoint)
        {
            if (!AllowUpdate) return;
            object[] content = new object[] { _pView.ViewID, projectileID, lookAtPoint};
            neManager.RaiseEvent(ByteEvents.PLAYER_TORPEDO_LAUNCH, content);
        }

        public void FireTorpedo(int projectileID, bool eventFire, Vector3 RELookatPoint)
        {
            if (!AllowUpdate) return;
            if (!eventFire && !tLauncher.IsChamberLoaded)
            {
                controller.UI.UpdateFiringVFX(true);
                return;
            }

            if (!eventFire)
            {
                projectileID += tLauncher.Data.ProjectileData.ProjTypeInt;
                torpedoFireAudioEvent.Invoke();
            }

            HandleTorpedoObject(projectileID, !eventFire, RELookatPoint);
        }
        private void HandleTorpedoObject(int projectileID, bool isLocal, Vector3 RELookatPoint)
        {
            //actual firing
            if (!tLauncher.IgnoreAmmo) tLauncher.DecrementChamber();
            UsableHandlerInfo info = CreateInfoForTorpedo(projectileID, tLauncher.IsPowered, isLocal);

            if (isLocal)
            {
                info = CalculateDeployAngle(info);
            }
            else
            {
                info.AimedPoint = RELookatPoint;
            }

            tLauncher.Use(info);
            controller.GetInfo.Inventory.IncreaseProjectileCount();

            //send event to torpedo ONLY when fire locally. local = (!eventFire)
            if (isLocal) SendTorpedoEvent(projectileID, info.AimedPoint);
        }

        public void FireUtility(int projectileID, UsableLauncherObject usable, int selectedItem , float chargeTime, bool isPowered ,bool eventFire, Vector3 RELookatPoint)
        {
            if (!eventFire && (!usable.IsChamberLoaded || !AllowUpdate))
                return;

            //actual firing
            HandleUtilityReloadTimer(usable);

            //why is this the case, you need to ask Jin or Jet because of network fuckery
            if(!eventFire)
            {
                projectileID += usable.Data.ProjectileData.ProjTypeInt;
            }

            //! Use utility here. If utility is used, decrement chamber! //
            UsableHandlerInfo info = CreateInfoForUtility(projectileID, isPowered, chargeTime, !eventFire);

            if (!eventFire)
            {
                info = CalculateDeployAngle(info);
            }             
            else
            {
                info.AimedPoint = RELookatPoint;
            }

            if (usable.Use(info))
            {
                if (!eventFire && !usable.IgnoreAmmo)
                    usable.DecrementChamber();
            }
            controller.GetInfo.Inventory.IncreaseProjectileCount();

            //send event to utility ONLY when fire locally. local = (!eventFire)
            if (!eventFire)
            {
                object[] content = new object[] { _pView.ViewID, projectileID, selectedItem, chargeTime, isPowered, info.AimedPoint};
                neManager.RaiseEvent(ByteEvents.PLAYER_UTILITIES_LAUNCH, content);
            }

        }

        private UsableHandlerInfo CreateInfoForTorpedo(int projectileID, bool isPowered, bool isLocal)
        {
            if (aimParentRb)
            {
                var info = new UsableHandlerInfo().WithTransformForceInfo(projectileID, isPowered, firePoint, 0f, aimParentRb.velocity, Vector3.zero, isLocal);
                info.OwnerObject = controller.GetTarget.gameObject;
                return info;
            }
            else
            {
                return null;
            }
        }
        private UsableHandlerInfo CreateInfoForUtility(int projectileID, bool isPowered, float chargedTime, bool isLocal)
        {
            if(aimParentRb)
            {
                var info = new UsableHandlerInfo().WithTransformForceInfo(projectileID, isPowered, firePoint, chargedTime, aimParentRb.velocity, Vector3.zero, isLocal);
                return info;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Enabling Component Methods

		public void SetCanFire(bool statement) => _canFire = statement;
        public bool AllowUpdate => _allowUpdate && _canFire;
        public void Enable() => _allowUpdate = true;
        public void Disable() => _allowUpdate = false;
        public void ToggleEnablility() => _allowUpdate = !_allowUpdate;

        #endregion

        #region UI

        private void OnChamberChangedMethod(bool isIncrement)
        {
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
            
            UpdateUITorpedoCount(false); 
        }
        private void OnReserveChangedMethod(bool isIncrement)
        {
            if (!isIncrement) return;
            DebugLog("Torpedo Regenerated (Loaded)!");
            
            UpdateUITorpedoCount(false); 
        }

        void OnRestock(UsableLauncherObject usableObject)
        {
           
        }
        private void OnUnityUpdateUI()
        {
            if (tLauncher.TotalAmmoCount > 0) UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
        }
        private void UpdateUITorpedoCount(bool isReloadEvent)
        {
            controller.UI.UpdateTorpedoReserve(tLauncher.TotalAmmoCount);
        }
        private void UpdateUIRegenRatio(in float ratio)
        {
            controller.UI.UpdateReload(ratio, tLauncher.IsRegenerating);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            controller.UI.UpdateTorpedoChamber(ratio, !tLauncher.IsChamberLoaded);
        }

        #endregion

        #region Timer

        private void BuildTimers()
        {
        }
        private void HandleUtilityReloadTimer(UsableLauncherObject usable)
        {
            usable.OnRestockInvoke();
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _pView = info.PhotonInfo.PView;
        }

        #endregion
    }
}