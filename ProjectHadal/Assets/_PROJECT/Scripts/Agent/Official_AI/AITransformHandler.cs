using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AITransformHandler : MonoBehaviour
    {
        [SerializeField] private List<Transform> AIComponents;
        public void Move(Vector3 worldPosition)
        {
            foreach (var child in AIComponents)
            {
                //! ignore self
                if (child == transform) continue;
                child.position = worldPosition;
            }
        }
		
		public void MoveAndRotate(Transform target)
		{
			foreach (var child in AIComponents)
			{
				//! ignore self
                if (child == transform) continue;
                child.position = target.position;
				child.rotation = target.rotation;
			}
		}
        
        public void MoveLocally(Vector3 localPosition)
        {
            foreach (var child in AIComponents)
            {
                //! ignore self
                if (child == transform) continue;
                child.localPosition = localPosition;
            }
        }
    }
}
