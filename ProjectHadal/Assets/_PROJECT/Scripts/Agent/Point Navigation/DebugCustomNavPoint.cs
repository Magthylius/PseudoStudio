using NaughtyAttributes;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugCustomNavPoint : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private Transform target;
        [SerializeField] PointNavigation navigator;

        [Header("Prefab")]
        [SerializeField] private NavPoint navPointPrefab;

        private void Start()
        {
            if (navPointPrefab == null)
                navPointPrefab = Resources.Load<GameObject>(PathManager.NavPointPrefabPath).GetComponent<NavPoint>();
            

        }

        [Button("Set Custom Waypoint")]
        private void SetCustomWaypoint()
        {
            NavPoint point = Instantiate(navPointPrefab, target.position, Quaternion.identity);
            navigator.SetCustomPath(point, false);
        }

        [Button("Set Custom Waypoint on Player")]
        private void SetCustomWaypointOnPlayer()
        {
            NavPoint point = Instantiate(navPointPrefab, target.position, Quaternion.identity);
            point.AttachTo(target);
            navigator.SetCustomPath(point, true);
        }

        [Button("Cancel Custom Waypoint")]
        private void CancelCustomWaypoint()
        {
            navigator.StopCustomPath();
        }
    }
}
