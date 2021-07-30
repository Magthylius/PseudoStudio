using System;
using System.Linq;
using Hadal.Player.Behaviours;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;
using Hadal.Inputs;
using Hadal.UI;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.Networking;
using ExitGames.Client.Photon;
using System.Collections;
using Hadal.Networking.UI.Loading;
using Hadal.Usables;
using Hadal.Locomotion;
using Hadal.AudioSystem;

// Created by Jet, E: Jon, Jin
namespace Hadal.Player
{
    public class PlayerController : Controller, IPlayerEnabler
    {
        #region Variable Definitions

        [SerializeField] private bool debugEnabled;
		internal bool DebugEnabled => debugEnabled;

        [Foldout("Components"), SerializeField] PlayerCameraController cameraController;
        [Foldout("Components"), SerializeField] PlayerHealthManager healthManager;
        [Foldout("Components"), SerializeField] PlayerInventory inventory;
        [Foldout("Components"), SerializeField] PlayerLamp lamp;
        [Foldout("Components"), SerializeField] PlayerShoot shooter;
        [Foldout("Components"), SerializeField] PlayerInteract interact;
        [Foldout("Components"), SerializeField] PlayerCollisions collisions;
        [Foldout("Components"), SerializeField] UIManager playerUI;
        [Foldout("Components"), SerializeField] DodgeBooster dodgeBooster;
        [Foldout("Components"), SerializeField] PlayerGraphicsHandler graphicsHandler;
        [Foldout("Components"), SerializeField] PlayerAudio playerAudio;

        [Foldout("Photon"), SerializeField] PlayerPhotonInfo photonInfo;
        [Foldout("Settings"), SerializeField] string localPlayerLayer;
        [Foldout("Physic Settings"), SerializeField] private PlayerPhysicData physicNormal;
        [Foldout("Physic Settings"), SerializeField] private PlayerPhysicData physicIncapacitated;
        [Foldout("Physic Settings"), SerializeField] private PlayerPhysicData physicHighGravityFriction;

        PhotonView _pView;
        PlayerManager _manager;
        Rigidbody _rBody;
        Collider _collider;

        private Color playerColor;
        
        private bool _isKnocked;
        private bool _isCarried;
        private bool _isTaggedByLeviathan;
        private bool _isDown;
        private bool _isLocalPlayer;
        public Action<PlayerController> LocalGameStartEvent;

        //! Ready checks
        bool playerReady = false;
        bool cameraReady = false;
        bool loadingReady = false;

        //Dummy System
        [SerializeField] bool isDummy = false;

        //! Self information
        Photon.Realtime.Player attachedPlayer;
        private PlayerClassType classType;

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

        public override void OnDisable()
        {
            if (_manager == null)
                return;
            
            if (!_manager.managerPView.IsMine) // If NOT the Host player, handle camera activation.
            {
                if (_pView.IsMine) // If camera started for a local player, send event to signify that its ready.
                {
                    LoadingManager.Instance.LoadingCompletedEvent.RemoveListener(SetLoadingReady);
                    NetworkEventManager.Instance.RemoveListener(ByteEvents.PLAYER_SPAWNED_CONFIRMED, PlayerReadyConfirmed);
                    NetworkEventManager.Instance.RemoveListener(ByteEvents.GAME_ACTUAL_START, StartGame);
                }
            }
        }
        void Start()
        {
            _rBody.maxDepenetrationVelocity = 1f; //! This is meant to make sure the collider does not penetrate too deeply into environmental collider (thus reducing bouncing)
            TryInjectDependencies();
            if (NetworkEventManager.Instance.isOfflineMode && !isDummy)
                SetLocalPlayerSettings();

            if (!_manager.managerPView.IsMine) // If NOT the Host player, handle camera activation.
            {
                HandlePhotonView(_pView.IsMine);

                if (_pView.IsMine) // If camera started for a local player, send event to signify that its ready.
                {
                    cameraReady = true;
                    // Debug.LogWarning("Added Listener For Loading!");
                    LoadingManager.Instance.LoadingCompletedEvent.AddListener(SetLoadingReady);
                    LoadingManager.Instance.AllowLoadingCompletion();
                    NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_SPAWNED_CONFIRMED, PlayerReadyConfirmed);
                    NetworkEventManager.Instance.AddListener(ByteEvents.GAME_ACTUAL_START, StartGame);
                    StartCoroutine(SendReady());
                }
            }

            NetworkEventManager.Instance.AddPlayer(gameObject);
            StartCoroutine(InitialiseData(() => OnInitialiseComplete?.Invoke(this)));
        }

        protected override void Update()
        {
            if (!_pView.IsMine || isDummy || !PlayerReadyForUpdateLoop) return;

            cameraController.CameraTransition(DeltaTime, IsBoosted);
            interact.DoUpdate(DeltaTime);
            inventory.DoUpdate(DeltaTime);
            lamp.DoUpdate(DeltaTime);
            healthManager.DoUpdate(DeltaTime);
            shooter.DoUpdate(DeltaTime);
            playerAudio.DoUpdate(DeltaTime);

            // mover for Vector Type.
            if (CanMove)
            {
                mover.DoUpdate(DeltaTime);
                dodgeBooster?.DoUpdate(DeltaTime);
            }
        }

        protected override void FixedUpdate()
        {
            if (!_pView.IsMine || isDummy || !PlayerReadyForUpdateLoop) return;

            if (CanMove) mover.DoFixedUpdate(FixedDeltaTime);
            if (CanMove) dodgeBooster?.DoFixedUpdate(FixedDeltaTime);
            if (CanRotate) rotator.DoFixedUpdate(FixedDeltaTime);
        }

        protected override void LateUpdate()
        {
            if (!_pView.IsMine || isDummy || !PlayerReadyForUpdateLoop) return;
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
        }

        IEnumerator InitialiseData(Action onInitialiseComplete)
        {
            while (_pView.ViewID == 0)
                yield return null;

            if (NetworkEventManager.Instance.IsMasterClient)
            {
                //! Host treats everything as isMine. This is bad, it relies on host first instantiate. Too bad!
                if (LocalPlayerData.PlayerController == null)
                {
                    LocalPlayerData.PlayerController = this;                
                    gameObject.AddComponent<AudioListener>();
                    Debug.LogWarning("Audio listener added");
                }
            }
            else if (_pView.IsMine)
            {
                LocalPlayerData.PlayerController = this;            
                gameObject.AddComponent<AudioListener>();
                Debug.LogWarning("Audio listener added");
            }

            NetworkData.AddPlayer(this);
            onInitialiseComplete?.Invoke();
            /*PlayerClassManager.Instance.ApplyClass();*/
        }

        public void SetPhysicDefault()
        {
            if (healthManager.IsDownOrUnalive) { SetPhysicHighFriction(); return; }
            SetPhysicNormal();

        }
        public void SetPhysicNormal() => physicNormal.SetPhysicDataForController(this);
        public void SetPhysicHighFriction() => physicHighGravityFriction.SetPhysicDataForController(this);
        public void SetPhysicIncapacitated() => physicIncapacitated.SetPhysicDataForController(this);

        public void SetIsCarried(in bool statement)
        {
            _isCarried = statement;
            if (_isCarried)
			{
				SetPhysicIncapacitated();
                if (IsLocalPlayer)
				{
                    playerAudio.PlayOneShot(PlayerSound.Grabbed, true);
				    playerAudio.AmbiencePlayer.PlayAmbienceOfType(AmbienceType.Grabbed_by_Leviathan);
                }
			}
            else
			{
				SetPhysicDefault();
				if (IsLocalPlayer)
                {
                    playerAudio.AmbiencePlayer.StopAmbienceOfType(AmbienceType.Grabbed_by_Leviathan);
                }
			}
        }
        public bool GetIsCarried => _isCarried;
        public void SetIsTaggedByLeviathan(in bool statement) => _isTaggedByLeviathan = statement;
        public bool GetIsTaggedByLeviathan => _isTaggedByLeviathan;
        public void SetIsDown(in bool statement) => _isDown = statement;

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
                if (cameraReady && loadingReady)
                {
                    print("sending my readiness!");
                    NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_SPAWNED, _pView.ViewID, SendOptions.SendReliable);
                }
                else
                {
                    Debug.LogWarning("CameraReady: " + cameraReady + "loadingReady: " + loadingReady) ;
                }
                yield return new WaitForSeconds(1);
            }

            Debug.LogWarning("All players are ready");
        }

        public void StartGame(EventData obj)
        {
            //! This is online called in online mode, this function is called on PlayerManager for host
            print("Everyone ready. Begin !");
            
            SetLocalPlayerSettings();
            PlayerClassManager.Instance.ApplyClass();
            if (PlayerClassManager.Instance.GetCurrentPlayerClass().ClassType == PlayerClassType.Informer)
            {
				playerAudio.EnableInRegister(PlayerSound.Informer_Whalesong);
				bool success = playerAudio.AmbiencePlayer.PlayAmbienceOfType(AmbienceType.Hydrophone_Whalesong);
				if (!success)
					"Ambience Player cannot be found in current scene.".Warn();
			}
            
            mover.ToggleEnablility(true);
            LoadingManager.Instance.StartEndLoad();
            _manager.InstantiatePViewList();
            TrackNamesOnline();
            LocalGameStartEvent?.Invoke(this);

            UpdateDiegetics();
        }

        public void TrackNamesOnline()
        {
            PlayerController[] allPlayerControllers = FindObjectsOfType<PlayerController>();

            NetworkEventManager neManager = NetworkEventManager.Instance;

            if (!neManager.isOfflineMode)
            {
                //! Track player names online, offline tracking called from player manager
                foreach (PlayerController controller in allPlayerControllers)
                {
                    foreach (var dict in neManager.GetSortedPlayerIndices())
                    {
                        if (controller._pView.Owner == dict.Key)
                        {
                            if (controller != this) playerUI.TrackPlayerName(controller.transform, dict.Key.NickName, neManager.GetPlayerClass(controller._pView.Owner));
                            controller.SetPlayerColor(neManager.GetPlayerColor(dict.Key));
                        }
                    }
                }
            }
        }

        public void TrackNamesOffline()
        {
            PlayerController[] allPlayerControllers = FindObjectsOfType<PlayerController>();
            //! Track player names offline
            foreach (PlayerController controller in allPlayerControllers)
            {
                //! ignore self
                if (controller == this) continue;
                
                playerUI.TrackPlayerName(controller.transform, controller.gameObject.name, PlayerClassType.Harpooner);
            }
        }

        public void UpdateDiegetics()
        {
            PlayerController[] allPlayerControllers = FindObjectsOfType<PlayerController>();
            foreach (PlayerController player in allPlayerControllers)
            {
                player.GraphicsHandler.ChangeEmissiveColor(NetworkEventManager.Instance.GetPlayerColor(player._pView.Owner));
            }
        }
        

        private void PlayerReadyConfirmed(EventData obj)
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
        }

        public void HandlePhotonView(bool isMine)
        {
            NetworkEventManager neManager = NetworkEventManager.Instance;
            
            if(!neManager.isOfflineMode)
            {
                gameObject.name = "Player " + photonInfo.PView.ViewID.ToString();
            }
            else
            {
                gameObject.name = "Player " + UnityEngine.Random.Range(0, 100);
                mover.ToggleEnablility(true);
                if (!isDummy)
                    SetLocalPlayerSettings();
            }
            
            if (UITrackerBridge.LocalPlayerUIManager == null && isMine)
            {
                //! Make sure player UI is inactive in prefab!
                playerUI.gameObject.SetActive(true);
                playerUI.InjectPlayer(pTrans, rotator, RotationInput);
                playerUI.PauseMenuOpened += Disable;
                playerUI.PauseMenuClosed += Enable;

                UITrackerBridge.LocalPlayerUIManager = playerUI;

                Activate();
                cameraController.Activate();
            }
            else
            {
                Deactivate();
                cameraController.Deactivate();

                try
                {
                    playerUI.PauseMenuOpened -= Disable;
                    playerUI.PauseMenuClosed -= Enable;
                }
                catch { }
            }

            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary> Calls when PhotonView.IsMine == true </summary>
        private void Activate()
        {

        }

        /// <summary> Calls when PhotonView.IsMine == false </summary>
        private void Deactivate()
        {

        }

        private void TryInjectDependencies()
        {
            if (_manager == null)
                _manager = PhotonView.Find((int)_pView.InstantiationData[0]).GetComponent<PlayerManager>();
        }
        
        public void InjectAIDependencies(Transform aiTransform)
        {
            playerUI.InjectAIDependencies(aiTransform);
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

        #region Player Mover/Rotator Changer (For Jin's physics)
        public void ChangeMoverToForce()
        {
            mover.Disable();
            mover = GetComponentInChildren<PlayerMovementF>();
            mover.Initialise(pTrans);
            mover.Enable();

            PhysicsHandler phyHandler = GetComponentInChildren<PhysicsHandler>();
            phyHandler.enabled = true;
            phyHandler.SetUpRigidBody();
        }

        public void ChangeMoverToVector()
        {
            mover.Disable();
            mover = GetComponentInChildren<PlayerMovementV>();
            mover.Initialise(pTrans);
            mover.Enable();

            _rBody.useGravity = false;
            _rBody.mass = 1;
            GetComponentInChildren<PhysicsHandler>().enabled = false;
            
        }

        public void ChangeMoverToHyrid()
        {
            mover.Disable();
            mover = GetComponentInChildren<PlayerMovementH>();
            mover.Initialise(pTrans);
            mover.Enable();

            _rBody.useGravity = false;
            _rBody.mass = 1;
            GetComponentInChildren<PhysicsHandler>().enabled = false;
        }

        public void ChangeRotatorToForce()
        {
            rotator.Disable();
            rotator = GetComponentInChildren<PlayerRotationF>();
            rotator.Initialise(pTrans);
            rotator.Enable();
        }

        public void ChangeRotatorToVector()
        {
            rotator.Disable();
            rotator = GetComponentInChildren<PlayerRotationV>();
            rotator.Initialise(pTrans);
            rotator.Enable();
        }

        public void ChangeRotatorToHyrid()
        {
            rotator.Disable();
            rotator = GetComponentInChildren<PlayerRotation>();
            rotator.Initialise(pTrans);
            rotator.Enable();
        }

        private void SetLocalPlayerSettings()
        {
            gameObject.layer = LayerMask.NameToLayer(localPlayerLayer);
            _isLocalPlayer = true;
            graphicsHandler.GraphicsObject.SetActive(false);
        }
        #endregion

        #region Shorthands
        public IMovementInput MovementInput => mover.Input;
        public IRotationInput RotationInput => rotator.Input;
        private float BoostInputSpeed => mover.Input.BoostAxis * mover.Accel.Boost + 1.0f;
        private bool IsBoosted => BoostInputSpeed > float.Epsilon + 1.0f;
        public Transform GetTarget => pTrans;
        public PlayerControllerInfo GetInfo
            => new PlayerControllerInfo(cameraController, healthManager, inventory, lamp,
                shooter, interact, photonInfo, mover, rotator, dodgeBooster, graphicsHandler, playerAudio, _rBody, _collider);
        public Photon.Realtime.Player AttachedPlayer => attachedPlayer;
        public int ViewID => _pView.ViewID;
        public bool CanMove => !_isKnocked && !_isCarried && !_isDown;
        public bool CanRotate => !_isDown && !_isCarried;
        public bool IsLocalPlayer => _isLocalPlayer;
        public PlayerClassType PlayerClass => classType;
        public void SetPlayerClass(PlayerClassType type) => classType = type;
        #endregion

        #region Accessors
        public void SetPlayerReady(bool isTrue) => playerReady = isTrue;
        public bool GetPlayerReady() => playerReady;
        public void SetLoadingReady() => loadingReady = true;
        public void SetDummyState(bool isTrue) => isDummy = isTrue;
        public void SetPlayerColor(Color newPlayerColor) => playerColor = newPlayerColor;
        public Color PlayerColor => playerColor;
        
        public string PlayerName => gameObject.name;
        public UIManager UI => playerUI;
        public PlayerGraphicsHandler GraphicsHandler => graphicsHandler;
        public bool PlayerReadyForUpdateLoop => playerReady || (NetworkEventManager.Instance != null && NetworkEventManager.Instance.isOfflineMode);
        #endregion
    }
}