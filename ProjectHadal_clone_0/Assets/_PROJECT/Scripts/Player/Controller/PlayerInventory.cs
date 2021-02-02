using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

//Created by Jet
namespace Hadal.Controls
{
    public class PlayerInventory : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Item[] items = null;
        private IEquipmentInput _eInput;
        private IUseableInput _uInput;
        private int _selectedItem;
        private int _previousSelectedItem = -1;
        private PhotonView _pView;
        private PlayerController _controller;

        private void Awake()
        {
            _eInput = new StandardEquipmentInput();
            _uInput = new StandardUseableInput();
        }

        public void Inject(PhotonView pView, PlayerController controller)
        {
            _pView = pView;
            _controller = controller;
            EquipItem(Equipment.Singleshot.DefaultIndex);
        }

        public void DoUpdate(in float deltaTime)
        {
            SelectItem();
            HandleItemInput();
        }

        private void SelectItem()
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                if (!_eInput.SlotIndex(i)) continue;
                EquipItem(i);
                break;
            }
        }

        private void EquipItem(int _index)
        {
            if (_index == _previousSelectedItem) return;

            _selectedItem = _index;
            items[_selectedItem].SetActiveState(true);
            if (_previousSelectedItem != -1) items[_previousSelectedItem].SetActiveState(false);
            _previousSelectedItem = _selectedItem;

            if (!_pView.IsMine) return;
            Hashtable hash = new Hashtable();
            hash.Add(nameof(_selectedItem), _selectedItem);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        private void HandleItemInput()
        {
            if (_uInput.FireKey1) items[_selectedItem].Use();
        }

        #region Pun Override

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (_pView == null) return;
            if (_pView.IsMine || targetPlayer != _pView.Owner) return;
            EquipItem((int)changedProps[nameof(_selectedItem)]);
        }

        #endregion
    }
}