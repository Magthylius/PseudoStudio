using NaughtyAttributes;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugCustomNavPoint : MonoBehaviour
    {
        [Header("Custom Path Info")]
        [SerializeField] private Transform target;
        [SerializeField] PointNavigationHandler navigator;
        
        [Header("Find Median Path Info")]
        [SerializeField] private NavPoint startPoint;
        [SerializeField] private NavPoint endPoint;

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

        [Button("Get Median Nav Point")]
        private void GetMedianNavPoint()
        {
            if (startPoint == null || endPoint == null) return;
            NavPoint p = startPoint.GetMedianNavPointTo(endPoint);
            if (p != null)
            {
                $"Median nav point is: {p.gameObject.name}".Msg();
            }
        }
    }
}
