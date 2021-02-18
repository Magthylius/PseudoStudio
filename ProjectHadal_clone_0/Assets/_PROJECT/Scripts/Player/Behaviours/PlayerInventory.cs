using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Hadal.Usables;
using Hadal.Inputs;
using Castle.Core.Internal;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerInventory : MonoBehaviourPunCallbacks
    {
        [SerializeField] private UsableObject[] utilities;
        private IEquipmentInput _eInput;
        private IUseableInput _uInput;
        private int _selectedItem;
        private int _previousSelectedItem = -1;
        private PhotonView _pView;
        private PlayerController _controller;
        private ControllerInfo _controllerInfo;

        private void Awake()
        {
            _eInput = new StandardEquipmentInput();
            _uInput = new StandardUseableInput();
        }

        public void Inject(PhotonView pView, PlayerController controller)
        {
            _pView = pView;
            _controller = controller;
            GetControllerInfo();
            InjectDependencies();
            EquipItem(0);
        }

        public void DoUpdate(in float deltaTime)
        {
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
            }
            if (_uInput.FireKey2)
            {
                _controllerInfo.Shooter.FireUtility(EquippedUsable);
            }
        }

        private void UpdateUsables(in float deltaTime)
        {
            int i = -1;
            while(++i < utilities.Length)
                utilities[i].DoUpdate(deltaTime);
        }

        private void EquipItem(int _index)
        {
            if (_index == _previousSelectedItem) return;
            _selectedItem = _index;
            ToggleItemActiveState();
            _previousSelectedItem = _selectedItem;

            if (!_pView.IsMine) return;
            UpdateNetworkItem();
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

        #region Shorthand

        public UsableObject[] GetUsableObjects => utilities;
        private UsableObject EquippedUsable => utilities[_selectedItem];

        #endregion
    }
}