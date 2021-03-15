using Tenshi;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Hadal.Usables;
using Hadal.Inputs;
using Hadal.Networking;
using Hadal.UI;

//Created by Jet, edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerInventory : MonoBehaviourPunCallbacks, IPlayerComponent, IPlayerEnabler
    {
        [SerializeField] private UsableLauncherObject[] utilities;
        private IEquipmentInput _eInput;
        private IUseableInput _uInput;
        private int _selectedItem;
        private int _previousSelectedItem = -1;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerControllerInfo _controllerInfo;

        [Header("Firing Variable")]
        private float _chargeTime = 0.0f;

        [Header("Event Code")]
        private const byte PLAYER_UTI_LAUNCH_EVENT = 2;

        NetworkEventManager neManager;

        private void Awake()
        {
            _eInput = new StandardEquipmentInput();
            _uInput = new StandardUseableInput();
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;
            if (neManager) neManager.AddListener(NetworkEventManager.ByteEvents.PLAYER_UTILITIES_LAUNCH, REFireUtility);

            EquipItem(0);
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _pView = info.PhotonInfo.PView;
            _controller = controller;
            GetControllerInfo();
            InjectDependencies();
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!AllowUpdate) return;
            SelectItem();
            HandleItemInput();
            UpdateUsables(deltaTime);
        }

        private void SelectItem()
        {
            if (utilities.IsNullOrEmpty()) return;
            for (int i = 0; i < utilities.Length; i++)
            {
                if (!_eInput.SlotIndex(i)) continue;
                EquipItem(i);
                break;
            }
        }

        private void HandleItemInput()
        {
            if (_uInput.FireKey1)
            {
                _controllerInfo.Shooter.FireTorpedo();
                _controllerInfo.Shooter.SendTorpedoEvent();
            }
            if (EquippedUsable.Data.isChargable)
            {
                if (_uInput.FireKey2Held)
                {
                    if (_chargeTime < 1f)
                    {
                        _chargeTime += EquippedUsable.Data.ChargingSpeed * Time.deltaTime;
                    }
                }
                if (_uInput.FireKey2Release)
                {
                    FireUtility();
                    _chargeTime = 0f;
                }
            }
            else if (_uInput.FireKey2)
            {
                FireUtility();
            }
        }
        
        //Fire when received Event
        void REFireUtility(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            if ((int)data[0] == _pView.ViewID)
            {
                _controllerInfo.Shooter.FireUtility(utilities[(int)data[1]],(float)data[2]);
            }
        }

        //Fire when pressed locally, send event
        void FireUtility()
        {
            _controllerInfo.Shooter.FireUtility(EquippedUsable, _chargeTime);
            object[] content = new object[] { _pView.ViewID, _selectedItem, _chargeTime };
            neManager.RaiseEvent(NetworkEventManager.ByteEvents.PLAYER_UTILITIES_LAUNCH, content);
        }

        private void UpdateUsables(in float deltaTime)
        {
            int i = -1;
            while(++i < utilities.Length)
                utilities[i].DoUpdate(deltaTime);
        }

        private void EquipItem(int _index)
        {
            bool sameIndex = _index == _previousSelectedItem;
            if (EquippedUsable.HasToggleAmmo && sameIndex)
            {
                EquippedUsable.ToggleAmmo();
                return;
            }

            if (_index == _previousSelectedItem) return;
            _selectedItem = _index;
            ToggleItemActiveState();
            _previousSelectedItem = _selectedItem;

            if (!_pView.IsMine) return;
            UpdateNetworkItem();
            print(UIManager.Instance);
            UIManager.Instance.UpdateCurrentUtility(EquippedUsable.UtilityName);
        }

        private void ToggleItemActiveState()
        {
            if (utilities.IsNullOrEmpty()) return;
            utilities[_selectedItem].Activate();
            if (_previousSelectedItem != -1)
                utilities[_previousSelectedItem].Deactivate();
        }

        private void UpdateNetworkItem()
        {
            Hashtable hash = new Hashtable();
            hash.Add(nameof(_selectedItem), _selectedItem);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        private void InjectDependencies()
        {
            var camera = _controllerInfo.CameraController.GetCamera;
            int i = -1;
            while (++i < utilities.Length)
            {
                utilities[i].Inject(camera);
                utilities[i].Deactivate();
            }
        }

        private void GetControllerInfo() => _controllerInfo = _controller.GetInfo;

        #region Pun Override

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (_pView == null) return;
            if (_pView.IsMine || targetPlayer != _pView.Owner) return;
            EquipItem((int)changedProps[nameof(_selectedItem)]);
        }

        #endregion

        #region Enabling Component Methods

        public bool AllowUpdate { get; private set; }
        public void Enable() => AllowUpdate = true;
        public void Disable() => AllowUpdate = false;
        public void ToggleEnablility() => AllowUpdate = !AllowUpdate;

        #endregion

        #region Shorthand

        public UsableLauncherObject[] GetUsableObjects => utilities;
        private UsableLauncherObject EquippedUsable => utilities[_selectedItem];

        #endregion
    }
}