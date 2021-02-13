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