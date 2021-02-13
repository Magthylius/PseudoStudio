using Hadal.Player.Behaviours;

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
        
        public ControllerInfo(PlayerCameraController camControl, PlayerHealthManager healthM, PlayerInventory inventory, PlayerLamp lamp, PlayerShoot shooter)
        {
            CameraController = camControl;
            HealthManager = healthM;
            Inventory = inventory;
            Lamp = lamp;
            Shooter = shooter;
        }
    }
}