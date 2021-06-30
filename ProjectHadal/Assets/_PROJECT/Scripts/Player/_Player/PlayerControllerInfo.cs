using Hadal.Locomotion;
using Hadal.Player.Behaviours;
using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Player
{
    public class PlayerControllerInfo
    {
        public PlayerCameraController CameraController { get; private set; }
        public PlayerHealthManager HealthManager { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerLamp Lamp { get; private set; }
        public PlayerShoot Shooter { get; private set; }
        public PlayerInteract Interact { get; private set; }
        public PlayerPhotonInfo PhotonInfo { get; private set; }
        public Mover Mover { get; private set; }
        public Rotator Rotator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Collider Collider { get; private set; }

        public PlayerControllerInfo(PlayerCameraController camControl, PlayerHealthManager healthM,
            PlayerInventory inventory, PlayerLamp lamp, PlayerShoot shooter, PlayerInteract interact,
            PlayerPhotonInfo pInfo, Mover mover, Rotator rotator, Rigidbody rigidbody, Collider collider)
        {
            CameraController = camControl;
            HealthManager = healthM;
            Inventory = inventory;
            Lamp = lamp;
            Shooter = shooter;
            Interact = interact;
            PhotonInfo = pInfo;
            Mover = mover;
            Rotator = rotator;
            Rigidbody = rigidbody;
            Collider = collider;
        }
    }
}