using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
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
		
		[Header("Information")]
        [SerializeField] private CavernTag cavernTag;
		[SerializeField] private bool isTunnelEntry;

		[Header("Friend references")]
        [SerializeField] private List<NavPoint> friends;
        public CavernTag CavernTag { get => cavernTag; set => cavernTag = value; }
        public Transform GetTransform => transform;
        public Vector3 GetPosition => transform.position;
        public float GetSqrDistanceTo(Vector3 position) => (position - GetPosition).sqrMagnitude;
        public Vector3 GetDirectionTo(Vector3 position) => (GetPosition - position).normalized;

        public void Initialise()
        {
            if (friends == null || friends.Count == 0)
                friends = new List<NavPoint>();
        }

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

        public NavPoint GetMedianNavPointTo(NavPoint end)
        {
            NavPoint theStart = this;
            NavPoint theEnd = end;
            if (theEnd == null) return null;

            List<NavPoint> path = new List<NavPoint>();
            NavPoint current = theStart;
            path.Add(current);

            while (current != theEnd && current != null)
            {
                if (current != null) current.Select();
                current = friends.OrderBy(o => current.GetSqrDistanceTo(theEnd.GetPosition)).FirstOrDefault();
                if (current != null) path.Add(current);
            }

            if (path.IsEmpty()) return null;

            int index = path.Count / 2;
            NavPoint median = path[index];
            return median;
        }

        
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
    }
}
