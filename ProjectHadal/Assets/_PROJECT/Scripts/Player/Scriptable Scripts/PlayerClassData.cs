using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Player.Behaviours;
using Hadal.Usables;
using UnityEngine;

namespace Hadal.Player
{
    [CreateAssetMenu(menuName = "Player/Class Data")]
    public class PlayerClassData : ScriptableObject
    {
        [Header("Utility Power Ups")]
        public bool GiveFlareHarpoon;
        public bool PowerUpFlare;
        public bool PowerUpHarpoon;

        [Header("Passive Power Ups")]
        public bool PowerUpDodgeBoost;
        public int DodgeBoostCount = 4;
        //
        public float ReviveTime = 1;

        //
        public bool PowerUpTorpFireRate;
        public float TorpFireRate = 0.1f;
        public string ClassName;

        [Header("Specialized Utility")]
        public List <UsableLauncherObject> ClassLauncher;

        public void SetUpUtility()
        {
            var playerInv = LocalPlayerData.PlayerController.GetInfo.Inventory;
            var playerBoost = LocalPlayerData.PlayerController.GetInfo.DodgeBooster;
            var playerTorpedo = LocalPlayerData.PlayerController.GetInfo.Shooter.GetTorpedoLauncher;
            playerInv.ResetEquipIndex();
            playerInv.DeactivateAllUtilities();
            playerInv.GetEquippedUsableObjects.Clear();

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

            foreach(UsableLauncherObject obj in ClassLauncher)
            {
                Type t = obj.GetType();
                AddUtility(t, playerInv);
            }

            if(PowerUpFlare)
            {
                foreach (UsableLauncherObject objLauncher in playerInv.GetEquippedUsableObjects)
                {
                    if (objLauncher is FlareLauncherObject)
                        objLauncher.IsPowered = true;
                }
            }

            if(PowerUpHarpoon)
            {
                foreach (UsableLauncherObject objLauncher in playerInv.GetEquippedUsableObjects)
                {
                    if (objLauncher is HarpoonLauncherObject)
                        objLauncher.IsPowered = true;
                }
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
        }
    }
}
