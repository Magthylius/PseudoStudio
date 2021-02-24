using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;

namespace FlyAgent.Navigation
{
	public class MapBaker : Singleton<MapBaker, DoNothing, SearchHierarchy>
	{
		[Header("World's")]
		[SerializeField] float m_MinNodeSize = 0.5f;
		[Tooltip("Clamped between 1 and 2. Values > 1 let nodes overlap.")]
		[SerializeField] [Range(1f, 2f)] float m_LoosenessVal = 1f;
		[SerializeField] float m_MinWorldSize = 10f;
		static readonly Color BAKE_COLOR = new Color(0f, 1f, .5f, .5f);

		[Header("Physical")]
		[SerializeField] LayerMask m_LayerMask = Physics.DefaultRaycastLayers;
		[SerializeField] QueryTriggerInteraction m_QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
		
		[SerializeField] bool m_ShownStaticBounds = false;
		[SerializeField] int m_ShowDepthOrder = 0;
		[SerializeField] bool m_ShownStaticObject = false;
		[SerializeField] bool m_ShownStaticCollision = false;
		[SerializeField] [Range(0f, 1f)] float m_BoundsAlphaStatic = 0.5f;

		Octree m_Octree = null;

		private void OnValidate()
		{
			if (m_MinNodeSize < 0.1f)
				m_MinNodeSize = 0.1f;

			if (m_ShowDepthOrder < 0)
				m_ShowDepthOrder = 0;
			if (m_Octree != null && m_ShowDepthOrder > m_Octree.totalDepth)
				m_ShowDepthOrder = m_Octree.totalDepth;
		}

		/// <summary>
        /// Note from Jet: Based on their 'TODO' comments, the baking must be done from scratch, so depending on the octree size, it
        /// might lag when the scene is loaded. They have not implemented cached backing where they just update and reload previously
        /// baked data. But since our game level is meant to be loaded once every playthrough, I think it is alright.
        /// </summary>
		// TODO: serialize result in scriptableObject and reload on Awake()
		protected override void Awake()
		{
			base.Awake();
			m_Octree = new Octree(m_MinWorldSize, transform.position, m_MinNodeSize, m_LoosenessVal);

			// TODO: bake & serialized the data
			BakeStatic();
		}

		/// <summary>
        /// Starts the update cycle of the octree
        /// <br/>
        /// Jet: UHHH, there is no <see cref="OnDisable()"/> function implementation. If we toggle the gameObject of this map baker
        /// on and off multiple times, I am not sure what will happen. You can try 
        /// </summary>
		private void OnEnable()
		{
			m_Octree.StartPeriodicUpdate(this);
		}

		/// <summary> Visualisation stuff for bounds of the octree </summary>
		private void OnDrawGizmos()
		{
			if (!Application.isPlaying &&
				m_Octree == null)
			{
				GizmosExtend.DrawBounds(new Bounds(transform.position, Vector3.one * m_MinWorldSize), BAKE_COLOR);
			}

			if (m_Octree != null && m_BoundsAlphaStatic > float.Epsilon)
			{
				if (m_ShownStaticBounds)
					m_Octree.DrawAllBounds(m_BoundsAlphaStatic, m_ShowDepthOrder);
				if (m_ShownStaticObject)
					m_Octree.DrawCost(m_BoundsAlphaStatic);
				if (m_ShownStaticCollision)
					m_Octree.DrawCollisionChecks();
			}
		}

		/// <summary> Assigns a found obstacle to the octree </summary>
		public void ReportObstacle(RaycastHit hit)
		{
			m_Octree.Add(hit.collider);
		}

		public Octree OctreeStatic
		{
			get { return m_Octree; }
		}

		/// <summary> Returns a list of nodes that are being fully collided by the provided bounds. </summary>
        /// <param name="point">I do not know why they have this, they do not use it at all lol</param>
		public List<Octree.Node> GetNearlyByObstacle(Vector3 point, Bounds bounds)
		{
			List<Octree.Node> result = new List<Octree.Node>(150);
			m_Octree.GetCollidingLeafNode(result, bounds);
			result.RemoveAll(o => !o.HasObstacle());
			return result;
		}
		
		/// <summary>
        /// Bakes a static octree in and automatically adds colliders that are found within the octree's bouds.
        /// </summary>
		[ContextMenu("Bake static")]
		public void BakeStatic()
		{
			Bounds world = new Bounds(transform.position, Vector3.one * m_MinWorldSize);
			m_Octree = new Octree(m_MinWorldSize, transform.position, m_MinNodeSize, m_LoosenessVal);
			Collider[] rst = Physics.OverlapBox(transform.position, world.extents, Quaternion.identity, m_LayerMask, m_QueryTriggerInteraction);
			for (int i = 0; i < rst.Length; i++)
			{
				m_Octree.Add(rst[i]);
			}
		}
	}
}