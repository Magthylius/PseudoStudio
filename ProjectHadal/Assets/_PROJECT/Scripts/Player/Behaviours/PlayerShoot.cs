//created by Jin, edited by Jon, edited by Jey
using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviourDebug, IPlayerComponent
    {
        [SerializeField] string debugKey;

        [Header("Aiming")]
        public Transform aimParentObject;
        public Transform aimPoint;

        float aimPointYDelta;
        Ray aimingRay;

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
        private const byte PLAYER_TOR_LAUNCH_EVENT = 1;

        #region Unity Lifecycle
        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
        }

        private void NetworkingClient_EventReceived(EventData obj)
        {
            if (obj.Code == PLAYER_TOR_LAUNCH_EVENT)
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

            aimingRay = new Ray(aimPoint.position, aimParentObject.forward * 1000f);
            aimPointYDelta = aimParentObject.position.y - aimPoint.position.y;
        }

        private void OnDestroy()
        {
            tLauncher.OnChamberChanged -= OnChamberChangedMethod;
            tLauncher.OnReservesChanged -= OnReserveChangedMethod;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawRay(aimingRay);
            Gizmos.DrawLine(aimPoint.position, aimParentObject.forward * 1000f);
        }

        public void DoUpdate(in float deltaTime)
        {
            OnUnityUpdateUI();
            CalculateTorpedoAngle();
        }

        #endregion

        #region Handler Methods
        public void CalculateTorpedoAngle()
        {
            RaycastHit aimHit;
            if (Physics.Raycast(aimingRay, out aimHit, Mathf.Infinity))
            {
                float angle = Mathf.Atan2(aimHit.point.y - aimPoint.position.y, aimHit.point.x - aimPoint.position.x);
                DebugLog(angle);
                return;
            }

            DebugLog("No angle");
        }


        public void FireTorpedo()
        {
            if (!tLauncher.IsChamberLoaded) return;
            HandleTorpedoObject();
        }

        //! Event Firing
        public void SendTorpedoEvent()
        {
            PhotonNetwork.RaiseEvent(PLAYER_TOR_LAUNCH_EVENT, _pView.ViewID, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }

        public void FireUtility(UsableLauncherObject usable)
        {
            if (!_canUtilityFire) return;
            HandleUtilityReloadTimer(usable);
            usable.Use(CreateInfoForUtility());
        }

        private void HandleTorpedoObject()
        {
            tLauncher.DecrementChamber();
            tLauncher.Use(CreateInfoForTorpedo());
        }

        private UsableHandlerInfo CreateInfoForTorpedo() => new UsableHandlerInfo().WithTransformInfo(torpedoFirePoint);
        private UsableHandlerInfo CreateInfoForUtility() => new UsableHandlerInfo().WithTransformInfo(utilityFirePoint);

        #endregion

        #region UI

        private void OnChamberChangedMethod(bool isIncrement)
        {
            UpdateUITorpedoCount();
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
            UpdateUITorpedoCount();
            if (!isIncrement) return;
            DebugLog("Torpedo Regenerated (Loaded)!");
        }
        private void OnUnityUpdateUI()
        {
            UpdateUIFloodRatio(tLauncher.ChamberReloadRatio);
            UpdateUIRegenRatio(tLauncher.ReserveRegenRatio);
        }
        private void UpdateUITorpedoCount()
        {
            UIManager.Instance
            .UpdateTubes(tLauncher.TotalTorpedoes);
        }
        private void UpdateUIRegenRatio(in float ratio)
        {
            UIManager.Instance
            .UpdateReload(ratio, tLauncher.IsRegenerating);
        }
        private void UpdateUIFloodRatio(in float ratio)
        {
            UIManager.Instance
            .UpdateFlooding(ratio, tLauncher.IsReloading);
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