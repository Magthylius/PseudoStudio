using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hadal
{
    public class ExplosivePoint : MonoBehaviour
    {
        public float RadiusOfEffect { get; private set; }
        public float ForceAmount { get; private set; }
        public bool DetonateAutomatically { get; private set; }
        public LayerMask IgnoreLayerMasks { get; private set; }
        private Vector3 GetPosition => transform.position;
        public event Action<ExplosionSettings> OnExplode;
        public event Action OnDespawn;

        private void Start()
        {
            if (!DetonateAutomatically)
                return;

            DetonateAndDestroy();
        }

        private void OnDestroy()
        {
            OnDespawn?.Invoke();
            OnDespawn = delegate { };
        }

        private void DetonateAndDestroy()
        {
            //! Get all eligible rigidbodies nearby
            List<Rigidbody> rigidbodies = Physics.OverlapSphere(GetPosition, RadiusOfEffect, ~IgnoreLayerMasks)
                                        .Select(x => x.GetComponent<Rigidbody>())
                                        .ToList();
            
            //! Cannot apply force to null or kinematic rigidbodies, therefore remove them
            rigidbodies.RemoveAll(r => r == null || r.isKinematic);

            rigidbodies.ForEach(r =>
            {
                //! determine direction of resultant force
                Vector3 forceDirection = r.transform.position - GetPosition;
                print("1. Force direction: " + forceDirection);
                
                //! Calculate distance between positions & get the ratio of distance relative to the total radius of effect
                float distance = Mathf.Clamp(Vector3.Distance(r.transform.position, GetPosition), 0f, RadiusOfEffect);
                print("2. Clamped distance: " + distance.ToString("F4"));

                float distanceRatio = distance / RadiusOfEffect;
                print("3. Distance ratio pre-check: " + distanceRatio.ToString("F4"));
                if (float.IsNaN(distanceRatio) || float.IsInfinity(distanceRatio)) //! make sure the value is not NaN or Infinity
                    distanceRatio = float.Epsilon;
                print("4. Distance ratio post-check: " + (distanceRatio == float.Epsilon ? "Epsilon" : distanceRatio.ToString("F4")));
                
                float inverseDistanceRatio = 1 - distanceRatio;
                print("5. Inverse distance ratio: " + inverseDistanceRatio.ToString("F4"));

                Vector3 force = forceDirection.normalized * ForceAmount * inverseDistanceRatio;
                print("6. Resultant force: " + force);

                r.AddForce(force, ForceMode.Impulse);
               /* r.AddTorque(force.magnitude * transform.up, ForceMode.Impulse);*/
            });
            
            OnExplode?.Invoke(GetSettingsForThisObject());
            OnExplode = delegate { };
            Destroy(gameObject);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, RadiusOfEffect);
        }

        private ExplosionSettings GetSettingsForThisObject()
        {
            return new ExplosionSettings
            {
                Position = GetPosition,
                Radius = RadiusOfEffect,
                Force = ForceAmount,
                DetonateOnRemote = !DetonateAutomatically,
                IgnoreLayers = IgnoreLayerMasks
            };
        }

        public void Detonate()
        {
            if (DetonateAutomatically)
                return;
            
            DetonateAndDestroy();
        }

        public static ExplosivePoint Create(ExplosionSettings settings)
        {
            ExplosivePoint prefab = Resources.Load<ExplosivePoint>(PathManager.ExplosivePointPrefabPath);
            ExplosivePoint point = Instantiate(prefab, settings.Position, Quaternion.identity);
            point.RadiusOfEffect = settings.Radius;
            point.ForceAmount = settings.Force;
            point.DetonateAutomatically = !settings.DetonateOnRemote;
            point.IgnoreLayerMasks = settings.IgnoreLayers;
            return point;
        }

        [Serializable]
        public class ExplosionSettings
        {
            public Vector3 Position = Vector3.zero;
            public float Radius = 30f;
            public float Force = 50f;
            public bool DetonateOnRemote = false;
            public LayerMask IgnoreLayers;
        }
    }
}
