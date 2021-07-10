//created by Jin, edited by Jon, edited by Jey

using System;
using UnityEngine;
using System.Collections;
using Hadal.Usables;
using Hadal.Utility;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hadal.UI;
using Hadal.Networking;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviourDebug, IPlayerComponent, IPlayerEnabler
    {
        [SerializeField] string debugKey;

        NetworkEventManager neManager;

        [Header("Player")]
        [SerializeField] PlayerController controller;
		private bool _allowUpdate;
		private bool _canFire;

        [Header("Aiming")]
        public Rigidbody aimParentRb;
        public Transform aimParentObject;
        public Transform aimPoint;
        public float torpedoMinAngle = 25f;
        public LayerMask rayIgnoreMask;
        private Ray aimingRay;
        float aimPointYDelta;
        
        //RaycastHit aimHit;
        //private bool aimHitBool;

        private bool enableTracer = false;

        [Header("Torpedo")]
        [SerializeField] TorpedoLauncherObject tLauncher;
        [SerializeField] Transform torpedoFirePoint;
        public TorpedoLauncherObject GetTorpedoLauncher => tLauncher;

        [Header("Utility")]
        [SerializeField] Transform utilityFirePoint;
        [SerializeField] float utilityFireDelay;
        private Timer _utilityReloadTimer;
        private bool _canUtilityFire;

        [Header("Event")]
        private PhotonView _pView;
        //private const byte PLAYER_TOR_LAUNCH_EVENT = 1;

        #region Unity Lifecycle
        private void OnEnable()
        {
            //PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
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
        }

        private void Start()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            DoDebugEnabling(debugKey);

            aimingRay = new Ray(aimPoint.position, aimParentObject.forward * 1000f);
            aimPointYDelta = (torpedoFirePoint.position - aimPoint.position).magnitude;

            // listen to salvage event, if local.
            if (NetworkEventManager.Instance.isOfflineMode)
            {
                if (controller == LocalPlayerData.PlayerController)
                {
                    Debug.LogWarning("Subscribed to Salvage");
                    tLauncher.SubscribeToSalvageEvent();
                    controller.UI.Initialize(tLauncher.TotalAmmoCount);
                }
            }
            else
            {
               // need to only subscribe if local
                Debug.LogWarning("Subscribed to Salvage");
                tLauncher.SubscribeToSalvageEvent();
                controller.UI.Initialize(tLauncher.TotalAmmoCount);
                
            }
        }
        
        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
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
            enableTracer = true;
        }

        public void StopShootTracer()
        {
            controller.UI.ShootTracer.Deactivate();
            enableTracer = false;
        }
        
        public UsableHandlerInfo CalculateTorpedoAngle(UsableHandlerInfo info)
        {
            /*if (aimHitBool)
            {
                info.AimedPoint = aimHit.point;
            }*/
            info.AimedPoint = controller.UI.ShootTracer.HitPoint;
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
                    FireTorpedo((int)data[1], true);
                }
            }
        }

        //! Event Firing
        public void SendTorpedoEvent(int projectileID)
        {
            if (!AllowUpdate) return;
            //PhotonNetwork.RaiseEvent(ByteEvents.PLAYER_TORPEDO_LAUNCH, _pView.ViewID, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            object[] content = new object[] { _pView.ViewID, projectileID};
            neManager.RaiseEvent(ByteEvents.PLAYER_TORPEDO_LAUNCH, content);
        }

        public void FireTorpedo(int projectileID, bool eventFire)
        {
            if (!AllowUpdate) return;
            if (!eventFire && !tLauncher.IsChamberLoaded)
            {
                //if (UIManager.IsNull) return;
                controller.UI.UpdateFiringVFX(true);
                return;
            }

            if (!eventFire)
            {
                projectileID += tLauncher.Data.ProjectileData.ProjTypeInt;
            }

            //send event to torpedo ONLY when fire locally. local = (!eventFire)
            if (!eventFire) SendTorpedoEvent(projectileID);

            HandleTorpedoObject(projectileID, !eventFire);
        }
        private void HandleTorpedoObject(int projectileID, bool isLocal)
        {
            //actual firing
            tLauncher.DecrementChamber();
            UsableHandlerInfo info = CreateInfoForTorpedo(projectileID, tLauncher.IsPowered, isLocal);
            info = CalculateTorpedoAngle(info);
            tLauncher.Use(info);
            controller.GetInfo.Inventory.IncreaseProjectileCount();
        }

        public void FireUtility(int projectileID, UsableLauncherObject usable, int selectedItem , float chargeTime, bool isPowered ,bool eventFire)
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
            if(usable.Use(CreateInfoForUtility(projectileID, isPowered, chargeTime, !eventFire)))
            {
                usable.DecrementChamber();
            }
            controller.GetInfo.Inventory.IncreaseProjectileCount();
            //send event to utility ONLY when fire locally. local = (!eventFire)
            if (!eventFire)
            {
                object[] content = new object[] { _pView.ViewID, projectileID, selectedItem, chargeTime, isPowered };
                neManager.RaiseEvent(ByteEvents.PLAYER_UTILITIES_LAUNCH, content);
            }

        }

        private UsableHandlerInfo CreateInfoForTorpedo(int projectileID, bool isPowered, bool isLocal)
        {
            if (aimParentRb)
            {
                //Debug.LogWarning("Rigidbody torpedo found");
                return new UsableHandlerInfo().WithTransformForceInfo(projectileID, isPowered, torpedoFirePoint, 0f, aimParentRb.velocity, Vector3.zero, isLocal);
            }
            else
            {
                //Debug.LogWarning("Rigidbody torpedo not found");
                return null;
            }
        }
        private UsableHandlerInfo CreateInfoForUtility(int projectileID, bool isPowered, float chargedTime, bool isLocal)
        {
            if(aimParentRb)
            {
                //Debug.LogWarning("Rigidbody utility found");
                return new UsableHandlerInfo().WithTransformForceInfo(projectileID, isPowered, utilityFirePoint, chargedTime, aimParentRb.velocity, Vector3.zero, isLocal);
            }
            else
            {
                //Debug.LogWarning("Rigidbody utility not found");
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
            UpdateUITorpedoCount(false);
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
            UpdateUITorpedoCount(isIncrement);
            if (!isIncrement) return;
            DebugLog("Torpedo Regenerated (Loaded)!");
        }
        private void OnUnityUpdateUI()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            UpdateUIRegenRatio(tLauncher.ReserveRegenRatio);

            /*aimHitBool = Physics.Raycast(aimPoint.position, aimParentObject.forward, out aimHit,
                Mathf.Infinity, ~rayIgnoreMask, QueryTriggerInteraction.Ignore);
                
            controller.UI.ShootTracer.SetEndPoint(aimHit.point);*/
        }
        private void UpdateUITorpedoCount(bool isReloadEvent)
        {
            //if (UIManager.IsNull) return;

            controller.UI.UpdateTubes(tLauncher.TotalAmmoCount, isReloadEvent);
        }
        private void UpdateUIRegenRatio(in float ratio)
        {
           // if (UIManager.IsNull) return;

            controller.UI.UpdateReload(ratio, tLauncher.IsRegenerating);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            //if (UIManager.IsNull) return;

            //controller.UI.UpdateFlooding(ratio, tLauncher.IsReloading);
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
        private void HandleUtilityReloadTimer(UsableLauncherObject usable)
        {
            _canUtilityFire = false;
            _utilityReloadTimer.Restart();
            usable.OnRestockInvoke();
        }
        private void SetCanUtilityFire() => _canUtilityFire = true;

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _pView = info.PhotonInfo.PView;
        }

        #endregion
    }
}