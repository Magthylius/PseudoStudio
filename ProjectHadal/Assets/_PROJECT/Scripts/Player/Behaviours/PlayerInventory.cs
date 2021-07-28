using Tenshi;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Hadal.Usables;
using Hadal.Inputs;
using Hadal.Networking;
using Hadal.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenshi.UnitySoku;

//Created by Jet, edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerInventory : MonoBehaviourPunCallbacks, IPlayerComponent, IPlayerEnabler
    {
        [Header("Utility Lists")]
        [SerializeField] private List<UsableLauncherObject> allUtilities;
        [SerializeField] private List<UsableLauncherObject> equippedUtilities;

        [Header("Settings")]
        [SerializeField] private bool enableQuickFlare;
        [SerializeField] private bool enableQuickHarpoon;
		
        private Dictionary<Type, UsableLauncherObject> quickFireUtilities;
        private IEquipmentInput _eInput;
        private IUseableInput _uInput;
        private int _selectedItem;
        private int _previousSelectedItem = -1;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerControllerInfo _controllerInfo;
        private int _projectileCount;
        private float _chargeTime = 0.0f;

        NetworkEventManager neManager;

        private FlareLauncherObject fLauncher;

        private void Awake()
        {
            _eInput = new StandardEquipmentInput();
            _uInput = new StandardUseableInput();
            quickFireUtilities = new Dictionary<Type, UsableLauncherObject>();
            allUtilities = GetComponentsInChildren<UsableLauncherObject>().ToList();

            var flareUtility = allUtilities.Where(u => u is FlareLauncherObject).Single();
            var harpoonUtility = allUtilities.Where(u => u is HarpoonLauncherObject).Single();
            harpoonUtility.OnReservesChanged += HandleHarpoonReserveChanged;
            flareUtility.OnReservesChanged += HandleFlareReserves;
            
            //! Sorry jet i have no idea why you want to make flareUtil a local var
            fLauncher = flareUtility as FlareLauncherObject;

            quickFireUtilities.Add(typeof(FlareLauncherObject), flareUtility);
            quickFireUtilities.Add(typeof(HarpoonLauncherObject), harpoonUtility);
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;
            if (neManager != null) neManager.AddListener(ByteEvents.PLAYER_UTILITIES_LAUNCH, REFireUtility);
            ResetEquipIndex();
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
            UpdateHarpoonChamberRatio();
        }

        /// <summary> Checks for keyboard input on the top row numbers, and switching the equip index to such. </summary>
        private void SelectItem()
        {
            if (equippedUtilities.IsNullOrEmpty()) return;
            for (int i = 0; i < equippedUtilities.Count; i++)
            {
                if (!_eInput.SlotIndex(i)) continue;
                EquipItem(i);
                break;
            }
        }

        /// <summary> Checks for inputs that are meant to trigger the torpedo or other utilities. </summary>
        private void HandleItemInput()
        {
            if (_controller.GetIsCarried)
                return;

            if (_uInput.FireKeyTorpedoDown)
            {
                _controllerInfo.Shooter.StartShootTracer();
            }
            if (_uInput.FireKeyTorpedoRelease)
            {
                _controllerInfo.Shooter.StopShootTracer();
                _controllerInfo.Shooter.FireTorpedo(pViewForProj + _projectileCount, false, Vector3.zero);
                return;
            }

            if (_uInput.FireKeyQuickFlare && enableQuickFlare)
                FireFlareWithShooter(pViewForProj + _projectileCount);

            if (_uInput.FireKeyQuickHarpoon && enableQuickHarpoon)
                FireHarpoonWithShooter(pViewForProj + _projectileCount);

            if (EquippedUsable != null && EquippedUsable.Data.isChargable)
            {
                if (_uInput.FireKeyUtilityHeld)
                {
                    if (_chargeTime < 1f)
                        _chargeTime += EquippedUsable.Data.ChargingSpeed * Time.deltaTime;
                }
                if (_uInput.FireKeyUtilityRelease)
                {
                    FireUtilityWithShooter(pViewForProj + _projectileCount);
                    _chargeTime = 0f;
                }
            }
            else if (_uInput.FireKeyUtility)
            {
                FireUtilityWithShooter(pViewForProj + _projectileCount);
            }
        }

        //Fire when received Event
        void REFireUtility(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            if ((int)data[0] == _pView.ViewID)
            {
                if (FindUtilityWithProjID((int)data[1]))
                {

                    //print(FindUtilityWithProjID((int)data[1]).UtilityName);
                    _controllerInfo.Shooter.FireUtility((int)data[1], FindUtilityWithProjID((int)data[1]), 0, (float)data[3], (bool)data[4], true, (Vector3)data[5]);
                    /*_controllerInfo.Shooter.FireUtility((int)data[1], utilities[(int)data[2]], 0, (float)data[3], true);*/
                }
                else
                {
                    print("Cant find the respective launcher");
                }
            }
        }

        void FireFlareWithShooter(int projectileID)
        {
            if (quickFireUtilities.TryGetValue(typeof(FlareLauncherObject), out var flareUtility))
            {
                flareUtility.Activate();
                _controllerInfo.Shooter.FireUtility(projectileID, flareUtility, -1, -1, flareUtility.IsPowered, false, Vector3.zero);
                return;
            }

            LogWarnMissingUsableLauncherInKeyValuePair("Flare");
        }

        void FireHarpoonWithShooter(int projectileID)
        {
            if (quickFireUtilities.TryGetValue(typeof(HarpoonLauncherObject), out var harpoonUtility))
            {
                harpoonUtility.Activate();
                _controllerInfo.Shooter.FireUtility(projectileID, harpoonUtility, -1, -1, harpoonUtility.IsPowered, false, Vector3.zero);
                return;
            }

            LogWarnMissingUsableLauncherInKeyValuePair("Harpoon");
        }

        //Fire when pressed locally, send event
        void FireUtilityWithShooter(int projectileID)
        {
            if (EquippedUsable == null) return;
            _controllerInfo.Shooter.FireUtility(projectileID, EquippedUsable, _selectedItem, _chargeTime, EquippedUsable.IsPowered, false, Vector3.zero);
        }

        void HandleFlareReserves(bool isIncrement)
        {
            Debug.LogWarning($"f laucnher: {fLauncher.TotalAmmoCount}");
            _controller.UI.UpdateFlareCount(fLauncher.TotalAmmoCount);
        }

        void UpdateHarpoonChamberRatio()
        {
            var harpoonUtility = quickFireUtilities[typeof(HarpoonLauncherObject)];
            _controller.UI.UpdateHarpoonChamber(harpoonUtility.ChamberReloadRatio);
        }

        void HandleHarpoonReserveChanged(bool isIncrement)
        {
            var harpoonUtility = quickFireUtilities[typeof(HarpoonLauncherObject)];
            _controller.UI.UpdateHarpoonReserve(harpoonUtility.ReserveCount);
        }

        private void LogWarnMissingUsableLauncherInKeyValuePair(string utilName)
            => $"Missing UsableLauncherObject for the {utilName.Bold()} in dictionary keyvaluepair.".Warn();

        private void UpdateUsables(in float deltaTime)
        {
            foreach (KeyValuePair<Type, UsableLauncherObject> pair in quickFireUtilities)
                pair.Value.DoUpdate(deltaTime);

            int i = -1;
            while (++i < equippedUtilities.Count)
                equippedUtilities[i].DoUpdate(deltaTime);
        }

        private void EquipItem(int _index)
        {
			if (EquippedUsable == null)
				return;
            if (EquippedUsable.isEquipLocked)
                return;

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
            // UpdateNetworkItem();

            _controller.UI.UpdateCurrentUtility(EquippedUsable.UtilityName);
        }

        //Find the correct LAUNCHER based on projID
        private UsableLauncherObject FindUtilityWithProjID(int projID)
        {
            string projTypeID = projID.ToString();
            //print("searching for" + projID);

            // return if projectile ID's length is less then 3, I.e., when its not shot by anyone.
            if (projTypeID.Length < 3)
                return null;

            //reduce the projID to 3 key words : The projTypeInt
            projTypeID = projTypeID.Substring(4, 1);
            projID = Convert.ToInt32(projTypeID);
            projID *= 100;
            //print("searching for" + projID);
            for (int i = 0; i < allUtilities.Count; i++)
            {
                if (allUtilities[i].Data.ProjectileData.ProjTypeInt == projID)
                {
                    DeactivateAllUtilities();
                    allUtilities[i].Activate();
                    return allUtilities[i];
                }
            }

            return null;
        }

        private void ToggleItemActiveState()
        {
            if (equippedUtilities.IsNullOrEmpty()) return;
            equippedUtilities[_selectedItem].Activate();
            if (_previousSelectedItem != -1)
                equippedUtilities[_previousSelectedItem].Deactivate();
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
            while (++i < equippedUtilities.Count)
            {
                equippedUtilities[i].Inject(camera);
                equippedUtilities[i].Deactivate();
            }
        }

        private void GetControllerInfo() => _controllerInfo = _controller.GetInfo;

        #region Pun Override

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            // if (_pView == null) return;
            // if (_pView.IsMine || targetPlayer != _pView.Owner) return;
            // EquipItem((int)changedProps[nameof(_selectedItem)]);
        }

        #endregion

        #region Enabling Component Methods

        public bool AllowUpdate { get; private set; }
        public void Enable() => AllowUpdate = true;
        public void Disable() => AllowUpdate = false;
        public void ToggleEnablility() => AllowUpdate = !AllowUpdate;

        #endregion

        #region Shorthand
        public int GetProjectileCount => _projectileCount;
        public void IncreaseProjectileCount()
        {
            if (_projectileCount > 20)
                _projectileCount = 0;
            else
                _projectileCount++;
        }
        public List<UsableLauncherObject> GetEquippedUsableObjects => equippedUtilities;
        public List<UsableLauncherObject> GetAllUsableObjects => allUtilities;

        /// <summary> Checks whether the inventory capacity for equipped utilities is reached.
        /// This is to prevent large inventory sizes. </summary>
        public bool MaxUtilityCapacityReached => equippedUtilities.Count >= allUtilities.Count;

        /// <summary> Sets the player's currently equipped item index to the very first slot. </summary>
        public void ResetEquipIndex() => EquipItem(0);

        /// <summary> Deactivates all utilities that are available in the list, <see cref="allUtilities"/>. </summary>
        public void DeactivateAllUtilities() => GetAllUsableObjects.ForEach(u => u.Deactivate());

        /// <summary> Adds a usable launcher object to the inventory's equipped list. </summary>
        /// <returns>Returns true if equipment is successfully added.</returns>
        public bool AddEquipment(UsableLauncherObject usable)
        {
            if (!MaxUtilityCapacityReached && !equippedUtilities.Contains(usable))
            {
                equippedUtilities.Add(usable);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a usable launcher object to the inventory's equipped list based on availability from the all utilities list.
        /// See <see cref="allUtilities"/>.
        /// </summary>
        /// <typeparam name="TLauncher">The class Type of a usable launcher object (e.g. <see cref="HarpoonLauncherObject"/>).</typeparam>
        /// <param name="manualActivate">Optional boolean to make sure the added usable is activated by default, if True.</param>
        /// <returns>Returns true if equipment is successfully added.</returns>
        public bool AddEquipmentOfType<TLauncher>(bool manualActivate = false) where TLauncher : UsableLauncherObject
        {
            for (int i = 0; i < allUtilities.Count; i++)
            {
                if (allUtilities[i] is TLauncher)
                {
                    bool status = AddEquipment(allUtilities[i]);
                    if (status)
                    {
                        if (manualActivate) allUtilities[i].Activate();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Another variant of the AddEquipmentOfType function. It is the same except it returns the object it is adding if successful, otherwise null.
        /// </summary>
        public UsableLauncherObject AddEquipmentOfType<TLauncher>() where TLauncher : UsableLauncherObject
        {
            for (int i = 0; i < allUtilities.Count; i++)
            {
                if (allUtilities[i] is TLauncher)
                {
                    bool status = AddEquipment(allUtilities[i]);
                    ToggleItemActiveState();
                    if (status)
                        return allUtilities[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a usable launcher object from the inventory's equipped list. Will return null if the type of launcher object is not found.
        /// </summary>
        public UsableLauncherObject GetEquipmentOfType<TLauncher>() where TLauncher : UsableLauncherObject
        {
            int i = -1;
            while (++i < equippedUtilities.Count)
            {
                if (equippedUtilities[i] is TLauncher)
                    return equippedUtilities[i];
            }
            return null;
        }

        /// <summary>
        /// Set the is powered status for any utility from the all utilities list.
        /// </summary>
        public void SetEquipmentIsPoweredStatus<TLauncher>(bool isPowered) where TLauncher : UsableLauncherObject
        {
            int i = -1;
            while (++i < allUtilities.Count)
            {
                if (allUtilities[i] is TLauncher)
                {
                    allUtilities[i].IsPowered = isPowered;
                    if (isPowered)
						allUtilities[i].PowerUp();
                }
            }
        }

        private UsableLauncherObject EquippedUsable => equippedUtilities.Count > 0 ? equippedUtilities[_selectedItem] : null;
        private int pViewForProj => _pView.ViewID * 1000;
        #endregion
    }
}