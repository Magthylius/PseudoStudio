using UnityEngine;

namespace Hadal.AI
{
    public class NavPoint : MonoBehaviour
    {
		private void Awake()
		{
			mRenderer = GetComponentInChildren<MeshRenderer>();
			if (mRenderer != null) defaultMaterial = mRenderer.material;
		}
		
		[Header("Debug")]
		[SerializeField] private Material selectedMaterial;
		private Material defaultMaterial;
		private MeshRenderer mRenderer;
		
		public void Select()
		{
			if (mRenderer == null) return;
			mRenderer.material = selectedMaterial;
		}
		public void Deselect()
		{
			if (mRenderer == null) return;
			mRenderer.material = defaultMaterial;
		}
		
		[Header("Information")]
        [SerializeField] private PointType pointType;
        public PointType GetPointType { get => pointType; set => pointType = value; }
        public Transform GetTransform => transform;
        public Vector3 GetPosition => transform.position;
        public float GetSqrDistanceTo(Vector3 position) => (position - GetPosition).sqrMagnitude;
        public Vector3 GetDirectionTo(Vector3 position) => (GetPosition - position).normalized;

        public void AttachTo(Transform target)
        {
            if (target == null)
            {
                transform.SetParent(null);
                return;
            }
            transform.position = target.position;
            transform.SetParent(target);
        }
    }

    public enum PointType
    {
        OpenArea,
        LairGrounds,
        HydrothermalCavern,
        BioluminescentCavern,
        Custom
    }
}
