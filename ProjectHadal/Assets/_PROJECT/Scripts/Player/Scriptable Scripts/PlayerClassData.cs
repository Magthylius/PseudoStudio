using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Player.Behaviours;
using Hadal.Usables;
using UnityEngine;
using Hadal.Networking;

namespace Hadal.Player
{
    [CreateAssetMenu(menuName = "Player/Class Data")]
    public class PlayerClassData : ScriptableObject
    {
        public string ClassName;
        [Header("Utility Power Ups")]
        public bool GiveFlareHarpoon;
        public bool PowerUpFlare;
        public bool PowerUpHarpoon;

        [Header("Passive Power Ups")]
        public bool PowerUpDodgeBoost;
        public int DodgeBoostCount = 3;
        public bool PowerUpReviveTime;
        public float ReviveTime = 1;

        //
        public bool PowerUpTorpFireRate;
        public float TorpFireRate = 0.1f;

        [Header("Specialized Utility")]
        public List <UsableLauncherObject> ClassLauncher;

        public void SetUpUtility()
        {
            var playerInv = LocalPlayerData.PlayerController.GetInfo.Inventory;
            var playerBoost = LocalPlayerData.PlayerController.GetInfo.DodgeBooster;
            var playerTorpedo = LocalPlayerData.PlayerController.GetInfo.Shooter.GetTorpedoLauncher;
            var playerHealth = LocalPlayerData.PlayerController.GetInfo.HealthManager;
            var playerPView = LocalPlayerData.PlayerController.GetInfo.PhotonInfo.PView;

            playerInv.ResetEquipIndex();
            playerInv.DeactivateAllUtilities();
            playerInv.GetEquippedUsableObjects.Clear();
            playerInv.SetEquipmentIsPoweredStatus<FlareLauncherObject>(PowerUpFlare);
            playerInv.SetEquipmentIsPoweredStatus<HarpoonLauncherObject>(PowerUpHarpoon);

            if(GiveFlareHarpoon)
            {
                playerInv.AddEquipmentOfType<FlareLauncherObject>(true);
                playerInv.AddEquipmentOfType<HarpoonLauncherObject>(true);
            }

            if(PowerUpDodgeBoost)
            {
                playerBoost.ChangeMaxReserveCount(DodgeBoostCount);
            }

            if(PowerUpTorpFireRate)
            {
                playerTorpedo.ChangeChamberReloadTime(TorpFireRate);
            }

            if(PowerUpReviveTime)
            {
                playerHealth.SetReviveOtherTime(ReviveTime);
                object[] content = new object[] { playerPView.ViewID, ReviveTime};
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_UPDATED_REVIVE_TIME, content);
            }

            foreach(UsableLauncherObject obj in ClassLauncher)
            {
                Type t = obj.GetType();
                AddUtility(t, playerInv);
            }
        }

        private void AddUtility(Type t, PlayerInventory playerInv)
        {
            if (t.Equals(typeof(TrapLauncherObject)))
            {
                playerInv.AddEquipmentOfType<TrapLauncherObject>(true);
            }
            else if (t.Equals(typeof(SonicDartLauncherObject)))
            {
                playerInv.AddEquipmentOfType<SonicDartLauncherObject>(true);
            }
            else if (t.Equals(typeof(SonicGrenadeLauncherObject)))
            {
                playerInv.AddEquipmentOfType<SonicGrenadeLauncherObject>(true);
            }
            else if (t.Equals(typeof(HarpoonLauncherObject)))
            {
                var harpoon = playerInv.AddEquipmentOfType<HarpoonLauncherObject>();
                harpoon.IsPowered = PowerUpHarpoon;
            }
            else if (t.Equals(typeof(FlareLauncherObject)))
            {
                var flare = playerInv.AddEquipmentOfType<FlareLauncherObject>();
                flare.IsPowered = PowerUpFlare;
            }
        }
    }
}
