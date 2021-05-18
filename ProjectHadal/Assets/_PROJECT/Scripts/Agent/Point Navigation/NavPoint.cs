using UnityEngine;

namespace Hadal.AI
{
    public class NavPoint : MonoBehaviour
    {
        [SerializeField] private PointType pointType;
        public PointType GetPointType { get => pointType; set => pointType = value; }
        public Transform GetTransform => transform;
        public Vector3 GetPosition => transform.position;
        public float GetSqrDistanceTo(Vector3 position) => (position - GetPosition).sqrMagnitude;
        public Vector3 GetDirectionTo(Vector3 position) => (GetPosition - position).normalized;
    }

    public enum PointType
    {
        Nest,
        OpenField,
        Volcanic,
        Crystal
    }
}
