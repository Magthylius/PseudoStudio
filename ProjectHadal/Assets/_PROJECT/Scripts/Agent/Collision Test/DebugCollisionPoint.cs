using System.Collections.Generic;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugCollisionPoint : MonoBehaviour
    {
        public bool useSphereCast;
        public GameObject fab;
        public float radius;
        public LayerMask mask;
        HashSet<Vector3> pointSet;

        private void Awake() => pointSet = new HashSet<Vector3>();

        private void Update()
        {
            if (!useSphereCast) return;
            RaycastHit[] rHits = Physics.SphereCastAll(transform.position, radius, transform.position, 1f, mask.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < rHits.Length; i++)
            {
                if (pointSet.Contains(rHits[i].point))
                    continue;
                
                pointSet.Add(rHits[i].point);
                Instantiate(fab, rHits[i].point, Quaternion.identity);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (useSphereCast) return;
            var pos = other.GetContact(0).point;
            Instantiate(fab, pos, Quaternion.identity);
            "collision".Msg();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (useSphereCast) return;
            Instantiate(fab, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
            "trigger".Msg();
        }
    }
}
