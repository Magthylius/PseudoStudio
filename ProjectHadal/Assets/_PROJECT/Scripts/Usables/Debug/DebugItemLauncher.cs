using UnityEngine;

namespace Hadal.Usables
{
    public class DebugItemLauncher : MonoBehaviour
    {
        [SerializeField] FlareLauncherData flareLau;
        [SerializeField] SonarDartLauncherData sonarLau;
        [SerializeField] SonicDartLauncherData sonicLau;

        [SerializeField] Transform target;

        [ContextMenu(nameof(LaunchFlare))]
        void LaunchFlare()
        {
            ItemHandlerInfo info = new ItemHandlerInfo(target.position, target.rotation);
            if (flareLau.DoEffect(info))
            {

            }
        }
        [ContextMenu(nameof(LaunchSonar))]
        void LaunchSonar()
        {

        }
        [ContextMenu(nameof(LaunchSonic))]
        void LaunchSonic()
        {

        }
    }
}