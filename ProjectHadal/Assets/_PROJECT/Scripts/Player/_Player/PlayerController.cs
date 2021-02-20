using System;
using System.Linq;
using Hadal.Player.Behaviours;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;
using Hadal.Inputs;

//Created by Jet
namespace Hadal.Player
{
    public class PlayerController : Controller
    {
        #region Variable Definitions

        [Foldout("Components"), SerializeField] private PlayerCameraController cameraController;
        [Foldout("Components"), SerializeField] private PlayerHealthManager healthManager;
        [Foldout("Components"), SerializeField] private PlayerInventory inventory;
        [Foldout("Components"), SerializeField] private PlayerLamp lamp;
        [Foldout("Components"), SerializeField] private PlayerShoot shooter;
        [Foldout("Components"), SerializeField] private PlayerCollisions collisions;
        [Foldout("Photon"), SerializeField] private PlayerPhotonInfo photonInfo;
        [Foldout("Settings"), SerializeField] private string localPlayerLayer;
        [Foldout("Graphics"), SerializeField] private GameObject[] graphics;
        [Foldout("Graphics"), SerializeField] private GameObject wraithGraphic;
        private PhotonView _pView;
        private PlayerManager _manager;
        
        public static event Action<PlayerController> OnInitialiseComplete;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _pView = photonInfo.PView;
            GetComponentsInChildren<IPlayerComponent>().ToList().ForEach(i => i.Inject(this));
        }

        private void Start()
        {
            TryInjectDependencies();
            HandlePhotonView(_pView.IsMine);
            OnInitialiseComplete?.Invoke(this);
        }

        protected override void Update()
        {
            DoAllUpdate(DeltaTime);
            DoLocalUpdate(DeltaTime);
        }

        private void OnCollisionEnter(Collision collision) => collisions.CollisionEnter(collision);
        private void OnCollisionStay(Collision collision) => collisions.CollisionStay(collision);

        #endregion

        #region Public Methods

        public void InjectManager(PlayerManager playerManager) => _manager = playerManager;

        public void AddVelocity(float speed, Vector3 direction)
        {
            Vector3 addVelocity = direction.normalized * speed;
            mover.Velocity.AddVelocity(addVelocity);
        }
        public void Die() => _manager.TryToDie();
        public void ResetController()
        {
            healthManager.Inject(this);
            healthManager.ResetManager();
        }

        #endregion

        #region Private Methods

        private void DoAllUpdate(in float deltaTime)
        {
            DebugCursor();
        }

        private void DoLocalUpdate(in float deltaTime)
        {
            if (!_pView.IsMine) return;
            cameraController.CameraTransition(deltaTime, IsBoosted);
            inventory.DoUpdate(deltaTime);
            lamp.DoUpdate(deltaTime);
            mover.DoUpdate(deltaTime);
            rotator.DoUpdate(deltaTime);
            shooter.DoUpdate(deltaTime);
        }

        private void HandlePhotonView(bool isMine)
        {
            if (isMine)
            {
                if (UIManager.Instance != null) UIManager.Instance.SetPlayer(this);
                gameObject.layer = LayerMask.NameToLayer(localPlayerLayer);
            }
            else
            {
                Deactivate();
                cameraController.Deactivate();
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

        #region Shorthands
        public IMovementInput MovementInput => mover.Input;
        public IRotationInput RotationInput => rotator.Input;
        private float BoostInputSpeed => mover.Input.BoostAxis * mover.Accel.Boost + 1.0f;
        private bool IsBoosted => BoostInputSpeed > float.Epsilon + 1.0f;
        public Transform GetTarget => pTrans;
        public PlayerControllerInfo GetInfo
            => new PlayerControllerInfo(cameraController, healthManager, inventory, lamp, shooter, photonInfo);

        #endregion
    }
}