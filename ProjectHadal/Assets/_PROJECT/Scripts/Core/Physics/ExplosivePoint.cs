using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hadal
{
    public class ExplosivePoint : MonoBehaviour
    {
        public int ExplosionDamage { get; private set; }
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
                // if its local player, damage them
                if(LayerMask.LayerToName(r.gameObject.layer) == "LocalPlayer")
                {
                    r.GetComponentInChildren<IDamageable>().TakeDamage(ExplosionDamage);
                }

                //! determine direction of resultant force
                Vector3 forceDirection = r.transform.position - GetPosition;
                
                //! Calculate distance between positions & get the ratio of distance relative to the total radius of effect
                float distance = Mathf.Clamp(Vector3.Distance(r.transform.position, GetPosition), 0f, RadiusOfEffect);

                float distanceRatio = distance / RadiusOfEffect;
                if (float.IsNaN(distanceRatio) || float.IsInfinity(distanceRatio)) //! make sure the value is not NaN or Infinity
                    distanceRatio = float.Epsilon;
                
                float inverseDistanceRatio = 1 - distanceRatio;

                Vector3 force = forceDirection.normalized * ForceAmount * inverseDistanceRatio;

                r.AddForce(force, ForceMode.Impulse);

                //!! remove this if you dont want rotation.
                Vector3 normalizedDirection = forceDirection.normalized;
                //Vector3 rotationVector = new Vector3(normalizedDirection.z, normalizedDirection.y, normalizedDirection.x);
                Vector3 rotationVector = new Vector3(0, normalizedDirection.x, 0);
                Debug.Log(rotationVector.x + ", " + rotationVector.y + ", " + rotationVector.z);
                r.GetComponentInChildren<IRotatable>()?.AddRotation(rotationVector, force.magnitude);

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
            point.ExplosionDamage = settings.Damage;
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
            public int Damage = 0;
            public float Radius = 30f;
            public float Force = 50f;
            public bool DetonateOnRemote = false;
            public LayerMask IgnoreLayers;
        }
    }
}
