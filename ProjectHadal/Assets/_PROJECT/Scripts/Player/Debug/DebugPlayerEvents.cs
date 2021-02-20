using System;
using Hadal.Player.Behaviours;
using Hadal.Usables;
using Hadal.Usables.Projectiles;
using UnityEngine;

namespace Hadal.Player
{
    public class DebugPlayerEvents : MonoBehaviourDebug
    {
        [SerializeField] string debugKey;
        PlayerInventory _inv;
        PlayerShoot _shoot;

        void Awake()
        {
            PlayerController.OnInitialiseComplete += GetController;
        }

        void Start()
        {
            DoDebugEnabling(debugKey);
        }

        void GetController(PlayerController controller)
        {
            PlayerController.OnInitialiseComplete -= GetController;
            _inv = controller.GetInfo.Inventory;
            _shoot = controller.GetInfo.Shooter;
            RegisterOnFireEvent();
        }

        void RegisterOnFireEvent()
        {
            foreach(var o in _inv.GetUsableObjects)
            {
                o.OnFire += CallOnFire;
            }
            _shoot.GetTorpedoLauncher.OnFire += CallOnFire;
        }

        void CallOnFire(UsableLauncherObject obj)
        {
            switch (obj)
            {
                case TorpedoLauncherObject t:
                    DebugLog("Torpedo Fire SFX");
                    return;
                case FlareLauncherObject f:
                    DebugLog("Flare Fire SFX");
                    return;
                case SonicDartLauncherObject s2:
                    DebugLog("Sonic Fire SFX");
                    return;
            }
        }
    }
}