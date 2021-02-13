//created by Jin
//edited by Jey
using UnityEngine;
using Hadal.Usables;
using Hadal.Utility;

namespace Hadal.Player.Behaviours
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] Transform firePoint;
        [SerializeField] float fireDelay;
        [SerializeField] float force;
        private Timer _reloadTimer;
        private bool _canFire;

        private void Awake()
        {
            _canFire = true;
            _reloadTimer = this.Create_A_Timer()
                        .WithDuration(fireDelay)
                        .WithOnCompleteEvent(SetCanFire)
                        .WithShouldPersist(true);
            _reloadTimer.Pause();
        }

        public void Fire(UsableObject usable)
        {
            if (!_canFire) return;
            HandleFireTimer();
            HandleUsable(usable);
        }

        private void HandleUsable(UsableObject usable)
        {
            if (usable is TorpedoLauncherObject torpedo)
            {
                if (!torpedo.IsChamberLoaded) return;
                torpedo.UnloadChamber();
                torpedo.Use(CreateInfo());
                return;
            }
            usable.Use(CreateInfo());
        }

        private void HandleFireTimer()
        {
            _canFire = false;
            _reloadTimer.Restart();
        }

        private UsableHandlerInfo CreateInfo() => new UsableHandlerInfo().WithTransformInfo(firePoint).WithForce(force);
        private void SetCanFire() => _canFire = true;
    }
}