//created by Jin, edited by Jon, edited by Jey
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
        RaycastHit aimHit;

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

        private void OnDisable()
        {
            //PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
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
        }

        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
        }

        void OnDrawGizmos()
        {
            //Gizmos.DrawRay(aimingRay);
            /*Gizmos.DrawLine(aimPoint.position, aimParentObject.forward * 1000f);

            if (Physics.Raycast(aimPoint.position, aimParentObject.forward, out aimHit))
            {
                Gizmos.DrawLine(aimPoint.position, aimHit.point);
                Gizmos.DrawLine(aimHit.point, torpedoFirePoint.position);
            }*/
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!AllowUpdate) return;
            OnUnityUpdateUI();
        }

        #endregion

        #region Handler Methods
        public UsableHandlerInfo CalculateTorpedoAngle(UsableHandlerInfo info)
        {
            if (Physics.Raycast(aimPoint.position, aimParentObject.forward, out aimHit,
                                Mathf.Infinity, ~rayIgnoreMask, QueryTriggerInteraction.Ignore))
            {
                info.AimedPoint = aimHit.point;
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

            HandleTorpedoObject(projectileID);
        }
        private void HandleTorpedoObject(int projectileID)
        {
            //actual firing
            tLauncher.DecrementChamber();
            UsableHandlerInfo info = CreateInfoForTorpedo(projectileID);
            info = CalculateTorpedoAngle(info);
            tLauncher.Use(info);
            controller.GetInfo.Inventory.IncreaseProjectileCount();
        }

        public void FireUtility(int projectileID, UsableLauncherObject usable, int selectedItem , float chargeTime, bool eventFire)
        {
            if (!eventFire && (!_canUtilityFire || !AllowUpdate))
                return;

            //actual firing
            HandleUtilityReloadTimer(usable);

            //why is this the case, you need to ask Jin or Jet because of network fuckery
            if(!eventFire)
            {
                projectileID += usable.Data.ProjectileData.ProjTypeInt;
            }
            
            usable.Use(CreateInfoForUtility(projectileID, chargeTime));
            controller.GetInfo.Inventory.IncreaseProjectileCount();

            //send event to utility ONLY when fire locally. local = (!eventFire)
            if (!eventFire)
            {
                object[] content = new object[] { _pView.ViewID, projectileID, selectedItem, chargeTime };
                neManager.RaiseEvent(ByteEvents.PLAYER_UTILITIES_LAUNCH, content);
            }

        }

        private UsableHandlerInfo CreateInfoForTorpedo(int projectileID)
        {
            if (aimParentRb)
            {
                Debug.LogWarning("Rigidbody torpedo found");
                return new UsableHandlerInfo().WithTransformForceInfo(projectileID, torpedoFirePoint, 0f, aimParentRb.velocity, Vector3.zero);
            }
            else
            {
                Debug.LogWarning("Rigidbody torpedo not found");
                return null;
            }

            return null;
        }
        private UsableHandlerInfo CreateInfoForUtility(int projectileID, float chargedTime)
        {
            if(aimParentRb)
            {
                Debug.LogWarning("Rigidbody utility found");
                return new UsableHandlerInfo().WithTransformForceInfo(projectileID, utilityFirePoint, chargedTime, aimParentRb.velocity, Vector3.zero);
            }
            else
            {
                Debug.LogWarning("Rigidbody utility not found");
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
        }
        private void UpdateUITorpedoCount(bool isReloadEvent)
        {
            //if (UIManager.IsNull) return;

            controller.UI.UpdateTubes(tLauncher.TotalTorpedoes, isReloadEvent);
        }
        private void UpdateUIRegenRatio(in float ratio)
        {
           // if (UIManager.IsNull) return;

            controller.UI.UpdateReload(ratio, tLauncher.IsRegenerating);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            //if (UIManager.IsNull) return;

            controller.UI.UpdateFlooding(ratio, tLauncher.IsReloading);
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