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

        [Header("Aiming")]
        public Rigidbody aimParentRb;
        public Transform aimParentObject;
        public Transform aimPoint;
        public float torpedoMinAngle = 25f;

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
            neManager.AddListener(ByteEvents.PLAYER_TORPEDO_LAUNCH, NetworkingClient_EventReceived);
        }

        private void OnDisable()
        {
            //PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
        }

        private void NetworkingClient_EventReceived(EventData obj)
        {
            if (obj.Code == (byte)ByteEvents.PLAYER_TORPEDO_LAUNCH)
            {
                if ((int)obj.CustomData == _pView.ViewID)
                {
                    FireTorpedo();
                }
            }
        }

        private void Awake()
        {
            BuildTimers();
            tLauncher.OnChamberChanged += OnChamberChangedMethod;
            tLauncher.OnReservesChanged += OnReserveChangedMethod;
        }

        private void Start()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            DoDebugEnabling(debugKey);

            //aimingRay = new Ray(aimPoint.position, aimParentObject.forward * 1000f);
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
            //Gizmos.DrawLine(aimPoint.position, aimParentObject.forward * 1000f);

            /*if (Physics.Raycast(aimPoint.position, aimParentObject.forward, out aimHit))
            {
                Gizmos.DrawLine(aimPoint.position, aimHit.point);
                Gizmos.DrawLine(aimHit.point, torpedoFirePoint.position);

                float o = (aimHit.point - aimPoint.position).magnitude;
                torpedoAngle = Mathf.Atan(o / aimPointYDelta) * Mathf.Rad2Deg;
               // DebugManager.Instance.SLog(sl_TorpedoAimer, "Torpedo aimer: ", Mathf.Atan(o / aimPointYDelta) * Mathf.Rad2Deg );
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
            if (Physics.Raycast(aimPoint.position, aimParentObject.forward, out aimHit))
            {
                float o = (aimHit.point - aimPoint.position).magnitude;
                float torpedoAngle = Mathf.Atan(o / aimPointYDelta) * Mathf.Rad2Deg;
                if (torpedoAngle < torpedoMinAngle) torpedoAngle = torpedoMinAngle;
                Vector3 newAngle = info.Orientation.eulerAngles - new Vector3(90f - torpedoAngle, 0f, 0f);
                info.Orientation = Quaternion.Euler(newAngle);
            }

            return info;
        }

        //! Event Firing
        public void SendTorpedoEvent()
        {
            if (!AllowUpdate) return;
            //PhotonNetwork.RaiseEvent(ByteEvents.PLAYER_TORPEDO_LAUNCH, _pView.ViewID, RaiseEventOptions.Default, SendOptions.SendUnreliable);
            neManager.RaiseEvent(ByteEvents.PLAYER_TORPEDO_LAUNCH, _pView.ViewID);
        }

        public void FireTorpedo()
        {
            if (!AllowUpdate) return;
            if (!tLauncher.IsChamberLoaded)
            {
                //if (UIManager.IsNull) return;
                controller.UI.UpdateFiringVFX(true);
                return;
            }
            HandleTorpedoObject();
        }
        private void HandleTorpedoObject()
        {
            tLauncher.DecrementChamber();
            UsableHandlerInfo info = CreateInfoForTorpedo();
            info = CalculateTorpedoAngle(info);
            tLauncher.Use(info);
        }

        public void FireUtility(UsableLauncherObject usable, float chargeTime)
        {
            if (!_canUtilityFire || !AllowUpdate) return;
            HandleUtilityReloadTimer(usable);
            usable.Use(CreateInfoForUtility(chargeTime));
        }
        
        private UsableHandlerInfo CreateInfoForTorpedo() => new UsableHandlerInfo().WithTransformForceInfo(torpedoFirePoint,0f, aimParentRb.velocity);
        private UsableHandlerInfo CreateInfoForUtility(float chargedTime) => new UsableHandlerInfo().WithTransformForceInfo(utilityFirePoint, chargedTime, aimParentRb.velocity);

        #endregion

        #region Enabling Component Methods

        public bool AllowUpdate { get; private set; }
        public void Enable() => AllowUpdate = true;
        public void Disable() => AllowUpdate = false;
        public void ToggleEnablility() => AllowUpdate = !AllowUpdate;

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