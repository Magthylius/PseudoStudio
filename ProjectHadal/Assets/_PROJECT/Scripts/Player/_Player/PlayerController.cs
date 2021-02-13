using Hadal.Player.Behaviours;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;

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
        [Foldout("Settings"), SerializeField] private SmoothNetworkPlayer networkPlayer;
        [Foldout("Settings"), SerializeField] private string localPlayerLayer;
        [Foldout("Graphics"), SerializeField] private GameObject[] graphics;
        [Foldout("Graphics"), SerializeField] private GameObject wraithGraphic;
        private PhotonView _pView;
        private PlayerManager _manager;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _pView = GetComponent<PhotonView>();
            InjectAwakeDependencies();
        }
        private void Start()
        {
            InjectStartDependencies();
            HandlePhotonView(_pView.IsMine);
        }

        protected override void Update()
        {
            DoAllUpdate(DeltaTime);
            DoOtherUpdate(DeltaTime);
            DoUpdate(DeltaTime);
        }

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
            healthManager.Inject(_pView, this, cameraController);
            healthManager.ResetManager();
        }

        #endregion

        #region Private Methods

        private void DoAllUpdate(in float deltaTime)
        {
            DebugCursor();
        }

        private void DoOtherUpdate(in float deltaTime)
        {
            if (_pView.IsMine || networkPlayer == null) return;
            networkPlayer.DoUpdate(deltaTime);
        }

        private void DoUpdate(in float deltaTime)
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
                if(UIManager.Instance != null) UIManager.Instance.SetPlayer(this);
                gameObject.layer = LayerMask.NameToLayer(localPlayerLayer);
                Destroy(networkPlayer);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Deactivate();
                cameraController.Deactivate();
            }

            wraithGraphic.SetActive(true);
            PhotonNetwork.RemoveBufferedRPCs(_pView.ViewID, nameof(RPC_SetPlayerGraphics));
            int randomIndex = Random.Range(0, graphics.Length);
            _pView.RPC(nameof(RPC_SetPlayerGraphics), RpcTarget.AllBuffered, randomIndex);
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

        private void InjectAwakeDependencies()
        {
            healthManager.Inject(_pView, this, cameraController);
            lamp.Inject(_pView);
        }
        private void InjectStartDependencies()
        {
            _manager ??= PhotonView.Find((int)_pView.InstantiationData[0]).GetComponent<PlayerManager>();
            inventory.Inject(_pView, this);
        }

        [PunRPC]
        private void RPC_SetPlayerGraphics(int index)
        {
            if (_pView.IsMine) return;
            graphics[index].SetActive(true);
        }

        #endregion

        #region Shorthands

        private float BoostInputSpeed => mover.Input.BoostAxis * mover.Accel.Boost + 1.0f;
        private bool IsBoosted => BoostInputSpeed > float.Epsilon + 1.0f;
        public Transform GetTarget => pTrans;
        public ControllerInfo GetInfo => new ControllerInfo(cameraController, healthManager, inventory, lamp, shooter);

        #endregion
    }
}