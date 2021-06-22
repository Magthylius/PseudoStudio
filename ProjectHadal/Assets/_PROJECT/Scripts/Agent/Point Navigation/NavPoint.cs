using System;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.AI
{
    public class NavPoint : MonoBehaviour
    {
		[Header("Debug")]
		[SerializeField] private bool disableGraphics;
		[SerializeField] private Material selectedMaterial;
		private Material defaultMaterial;
		private MeshRenderer mRenderer;

		[Header("Approaching vectors")] 
		public NavPoint approachPoint;
		
		[Header("Information")]
        [SerializeField] private CavernTag cavernTag;
		[SerializeField] private bool isTunnelEntry;
		public CavernTag CavernTag => cavernTag;
        public bool IsTunnelEntry => isTunnelEntry;

		[Header("Friend references")]
        [SerializeField] private List<NavPoint> friends;
		public Transform GetTransform => transform;
        public Vector3 GetPosition => transform.position;
        public float GetSqrDistanceTo(Vector3 position) => (position - GetPosition).sqrMagnitude;
        public Vector3 GetDirectionTo(Vector3 position) => (GetPosition - position).normalized;

		[Button("Disable All")]
		private void Button_DisableAll()
		{
			NavPoint.DisableGraphicsAll();
		}
		[Button("Enable All")]
		private void Button_EnableAll()
		{
			NavPoint.EnableGraphicsAll();
		}

        void OnValidate()
        {
	        if (approachPoint != null)
	        {
		        approachPoint.SetCavernTag(cavernTag);
		        approachPoint.SetIsTunnelEntry(true);
	        }
        }

        private void OnDrawGizmosSelected()
        {
	        if (approachPoint != null)
	        {
	            approachPoint.transform.LookAt(transform);
		        Gizmos.DrawLine(transform.position, approachPoint.GetPosition);
	        }
        }
		
		private void Awake()
		{
			mRenderer = GetComponentInChildren<MeshRenderer>();
			if (mRenderer != null) defaultMaterial = mRenderer.material;
		}

        public void Initialise()
        {
            if (friends == null || friends.Count == 0)
                friends = new List<NavPoint>();
			
			if (disableGraphics)
			{
				transform.GetChild(0).gameObject.SetActive(false);
			}
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
        
        public void Select()
		{
			if (mRenderer == null || disableGraphics) return;
			mRenderer.material = selectedMaterial;
		}
		public void Deselect()
		{
			if (mRenderer == null || disableGraphics) return;
			mRenderer.material = defaultMaterial;
		}

		public void SetCavernTag(CavernTag newTag)
		{
			//Debug.LogWarning(newTag);
			cavernTag = newTag;
		}

		public void SetIsTunnelEntry(bool isEntry) => isTunnelEntry = isEntry;
		public void SetDisableGraphics(bool state) => disableGraphics = state;
		
		public static void DisableGraphicsAll()
		{
			var points = FindObjectsOfType<NavPoint>();
			int i = -1;
			while (++i < points.Length)
			{
				points[i].SetDisableGraphics(true);
				if (points[i].transform.childCount != 0)
					points[i].transform.GetChild(0).gameObject.SetActive(false);
			}
		}
		public static void EnableGraphicsAll()
		{
			var points = FindObjectsOfType<NavPoint>();
			int i = -1;
			while (++i < points.Length)
			{
				points[i].SetDisableGraphics(false);
				points[i].transform.GetChild(0).gameObject.SetActive(true);
			}
		}
    }
}
