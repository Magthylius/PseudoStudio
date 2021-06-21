using System;
using System.Linq;
using Hadal.Player.Behaviours;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;
using Hadal.Inputs;
using Hadal.UI;
using Tenshi;
using Hadal.Networking;
using ExitGames.Client.Photon;
using System.Collections;
using Hadal.Networking.UI.Loading;

// Created by Jet, E: Jon, Jin
namespace Hadal.Player
{
    public class PlayerController : Controller, IPlayerEnabler
    {
        #region Variable Definitions

        [Foldout("Components"), SerializeField] PlayerCameraController cameraController;
        [Foldout("Components"), SerializeField] PlayerHealthManager healthManager;
        [Foldout("Components"), SerializeField] PlayerInventory inventory;
        [Foldout("Components"), SerializeField] PlayerLamp lamp;
        [Foldout("Components"), SerializeField] PlayerShoot shooter;
        [Foldout("Components"), SerializeField] PlayerCollisions collisions;
        [Foldout("Components"), SerializeField] UIManager playerUI;

        [Foldout("Photon"), SerializeField] PlayerPhotonInfo photonInfo;
        [Foldout("Settings"), SerializeField] string localPlayerLayer;
        [Foldout("Graphics"), SerializeField] GameObject[] graphics;
        [Foldout("Graphics"), SerializeField] GameObject wraithGraphic;

        PhotonView _pView;
        PlayerManager _manager;
        Rigidbody _rBody;
        Collider _collider;

        private bool _isKnocked;
        private bool _isCarried;

        //! Ready checks
        bool playerReady = false;
        bool cameraReady = false;
        bool loadingReady = false;

        //Dummy System
        [SerializeField] bool isDummy = false;

        //! Self information
        Photon.Realtime.Player attachedPlayer;
        int pViewSelfID;

        public static event Action<PlayerController> OnInitialiseComplete;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _pView = photonInfo.PView;
            _rBody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _isKnocked = false;
            _isCarried = false;
            GetComponentsInChildren<IPlayerComponent>().ToList().ForEach(i => i.Inject(this));
            var self = GetComponent<IPlayerEnabler>();
            enablerArray = GetComponentsInChildren<IPlayerEnabler>().Where(i => i != self).ToArray();
            Enable();
        }

        void Start()
        {
            //base.OnEnable();
            TryInjectDependencies();

            if (!_manager.managerPView.IsMine) // If NOT the Host player, handle camera activation.
            {
                HandlePhotonView(_pView.IsMine);

                if (_pView.IsMine) // If camera started for a local player, send event to signify that its ready.
                {
                    cameraReady = true;
                    //print("Loading set listener");
                    LoadingManager.Instance.LoadingCompletedEvent.AddListener(SetLoadingReady);
                    LoadingManager.Instance.AllowLoadingCompletion();
                    NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_SPAWNED_CONFIRMED, playerReadyConfirmed);
                    NetworkEventManager.Instance.AddListener(ByteEvents.START_THE_GAME, StartGame);
                    StartCoroutine(SendReady());
                }
            }

            OnInitialiseComplete?.Invoke(this);
            NetworkEventManager.Instance.AddPlayer(gameObject);
            //LocalPlayerData.ViewID = pViewSelfID;
            StartCoroutine(InitializeData());
            //Deactivate();
        }

        protected override void Update()
        {
            DoDebugUpdate(DeltaTime);

            if (!_pView.IsMine) return;
            if (isDummy) return;

            cameraController.CameraTransition(DeltaTime, IsBoosted);
            inventory.DoUpdate(DeltaTime);
            lamp.DoUpdate(DeltaTime);
            healthManager.DoUpdate(DeltaTime);
            shooter.DoUpdate(DeltaTime);
        }

        protected override void FixedUpdate()
        {
            if (!_pView.IsMine) return;
            if (isDummy) return;

            if (CanMove) mover.DoFixedUpdate(FixedDeltaTime);
            if (CanRotate) rotator.DoFixedUpdate(FixedDeltaTime);
        }

        protected override void LateUpdate()
        {
            if (!_pView.IsMine) return;
        }

        private void OnCollisionEnter(Collision collision) => collisions.CollisionEnter(collision);
        private void OnCollisionStay(Collision collision) => collisions.CollisionStay(collision);
        private void OnCollisionExit(Collision collision) => collisions.CollisionExit(collision);

        void OnDestroy()
        {
            //! Might need to uninject player
            playerUI.PauseMenuOpened -= Disable;
            playerUI.PauseMenuClosed -= Enable;
        }

        #endregion

        #region Public Methods
        public void InjectDependencies(PlayerManager playerManager, Photon.Realtime.Player photonPlayer)
        {
            _manager = playerManager;
            attachedPlayer = photonPlayer;

            pViewSelfID = _pView.ViewID;
        }

        IEnumerator InitializeData()
        {
            while (_pView.ViewID == 0)
            {
                yield return null;
            }

            if (_pView.IsMine)
            {
                LocalPlayerData.PlayerController = this;
            }
            
            NetworkData.AddPlayer(this);
            Debug.LogWarning("Network Data updated: " + NetworkData.PlayerCount);
            //Debug.LogWarning(pViewSelfID);
        }

        public void SetIsCarried(in bool statement) => _isCarried = statement;
        
        public void Die() => _manager.TryToKill(attachedPlayer);
        public void ResetController()
        {
            healthManager.Inject(this);
            healthManager.ResetManager();
        }

        public void EnableCollider() => _collider.enabled = true;
        public void DisableCollider() => _collider.enabled = false;
        #endregion

        #region Private Methods

        /// <summary>
        /// Notifies ready co-routine
        /// </summary>
        IEnumerator SendReady()
        {
            while (!playerReady)
            {
                //print(cameraReady && loadingReady);
                if (cameraReady && loadingReady)
                {
                    //print("event sent");
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_SPAWNED, _pView.ViewID, SendOptions.SendReliable);
                }
                yield return new WaitForSeconds(1);
            }
        }


        private void DoDebugUpdate(in float deltaTime)
        {
            DebugCursor();
        }

        private void StartGame(EventData obj)
        {
            print("Everyone ready. Begin !");
            LoadingManager.Instance.StartEndLoad();
            _manager.instantiatePViewList();
        }

        private void playerReadyConfirmed(EventData obj)
        {
            if (!_pView)
                return;

            if (_pView.ViewID == (int)obj.CustomData)
            {
                print("You readiness is recognized.");
                playerReady = true;
            }
        }

        public void TransferOwnership(Photon.Realtime.Player newOwner)
        {
            _pView.TransferOwnership(newOwner);
            /*print("Transfer: " + newOwner.NickName + ", " + _pView.IsMine);
            print(NetworkEventManager.Instance.LocalPlayer.NickName);
            print(newOwner.NickName);
            if (NetworkEventManager.Instance.LocalPlayer == newOwner)
            {
                print("Transfer: " + newOwner.NickName + " handling");
                HandlePhotonView(true);
            }*/
        }

        public void HandlePhotonView(bool isMine)
        {
            gameObject.layer = LayerMask.NameToLayer(localPlayerLayer);
            
            if(!NetworkEventManager.Instance.isOfflineMode)
            {
                gameObject.name = "Player " + photonInfo.PView.ViewID.ToString();
            }
            else
            {
                gameObject.name = "Player " + UnityEngine.Random.Range(0, 100);
            }
            
            if (isMine)
            {
                //! Make sure player UI is inactive in prefab!
                playerUI.gameObject.SetActive(true);
                playerUI.InjectPlayer(pTrans, rotator, RotationInput);
                playerUI.PauseMenuOpened += Disable;
                playerUI.PauseMenuClosed += Enable;

                Activate();
                cameraController.Activate();

                print("Camera Activated");
            }
            else
            {
                Deactivate();
                cameraController.Deactivate();
                print("Camera Deactivated");

                try
                {
                    playerUI.PauseMenuOpened -= Disable;
                    playerUI.PauseMenuClosed -= Enable;
                }
                catch { }
            }

            Cursor.lockState = CursorLockMode.Locked;
            SetGraphics();
        }

        private void SetGraphics()
        {
            // wraithGraphic.SetActive(true);
            // PhotonNetwork.RemoveBufferedRPCs(_pView.ViewID, nameof(RPC_SetPlayerGraphics));
            // int randomIndex = UnityEngine.Random.Range(0, graphics.Length);
            // _pView.RPC(nameof(RPC_SetPlayerGraphics), RpcTarget.AllBuffered, randomIndex);
        }

        private void Activate()
        {

        }

        private void Deactivate()
        {

        }

        private void DebugCursor()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void TryInjectDependencies()
        {
            _manager ??= PhotonView.Find((int)_pView.InstantiationData[0]).GetComponent<PlayerManager>();
        }

        [PunRPC]
        private void RPC_SetPlayerGraphics(int index)
        {
            if (!_pView.IsMine) return;
            graphics[0].SetActive(true);
        }

        #endregion

        #region IPlayerEnabler Methods

        public bool AllowUpdate { get; private set; }
        private IPlayerEnabler[] enablerArray;
        public void Enable()
        {
            if (enablerArray.IsNullOrEmpty()) return;
            AllowUpdate = true;
            enablerArray.ToList().ForEach(i => i.Enable());
            mover.Enable();
            rotator.Enable();
        }
        public void Disable()
        {
            if (enablerArray.IsNullOrEmpty()) return;
            AllowUpdate = false;
            enablerArray.ToList().ForEach(i => i.Disable());
            mover.Disable();
            rotator.Disable();
        }
        public void ToggleEnablility()
        {
            AllowUpdate = !AllowUpdate;
            if (AllowUpdate)
            {
                Enable();
                return;
            }
            Disable();
        }

        #endregion

        #region Shorthands
        public IMovementInput MovementInput => mover.Input;
        public IRotationInput RotationInput => rotator.Input;
        private float BoostInputSpeed => mover.Input.BoostAxis * mover.Accel.Boost + 1.0f;
        private bool IsBoosted => BoostInputSpeed > float.Epsilon + 1.0f;
        public Transform GetTarget => pTrans;
        public PlayerControllerInfo GetInfo
            => new PlayerControllerInfo(cameraController, healthManager, inventory, lamp, shooter, photonInfo, mover, rotator, _rBody);
        public Photon.Realtime.Player AttachedPlayer => attachedPlayer;
        public int ViewID => pViewSelfID;
        public bool CanMove => !_isKnocked && !_isCarried;
        public bool CanRotate => true;

        #endregion

        #region Accessors
        public void setPlayerReady(bool isTrue)
        {
            playerReady = isTrue;
        }

        public bool getPlayerReady()
        {
            return playerReady;
        }

        public void SetLoadingReady()
        {
            loadingReady = true;
            //print("Loading Ready is : " + loadingReady);
        }

        public void SetDummyState(bool isTrue)
        {
            isDummy = isTrue;
        }
        public string PlayerName => gameObject.name;
        public UIManager UI => playerUI;
        #endregion
    }
}