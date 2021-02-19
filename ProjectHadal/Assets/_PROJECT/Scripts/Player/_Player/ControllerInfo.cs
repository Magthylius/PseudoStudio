﻿using Hadal.Player.Behaviours;
using Photon.Pun;

//Created by Jet
namespace Hadal.Player
{
    public class ControllerInfo
    {
        public PlayerCameraController CameraController { get; private set; }
        public PlayerHealthManager HealthManager { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerLamp Lamp { get; private set; }
        public PlayerShoot Shooter { get; private set; }
        public PhotonView PhotonView { get; private set; }

        public ControllerInfo(PlayerCameraController camControl, PlayerHealthManager healthM, PlayerInventory inventory, PlayerLamp lamp, PlayerShoot shooter, PhotonView pView)
        {
            CameraController = camControl;
            HealthManager = healthM;
            Inventory = inventory;
            Lamp = lamp;
            Shooter = shooter;
            PhotonView = pView;
        }
    }
}