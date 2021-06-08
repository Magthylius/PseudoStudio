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
            List<Rigidbody> rigidbodies = Physics.OverlapSphere(GetPosition, RadiusOfEffect)
                                        .Where(x => x.GetComponent<Rigidbody>() != null)
                                        .Select(x => x.GetComponent<Rigidbody>())
                                        .ToList();
            rigidbodies.ForEach(r =>
            {
                Vector3 force = (GetPosition - r.transform.position).normalized * ForceAmount;
                r.AddForce(force, ForceMode.Impulse);
            });
            
            OnExplode?.Invoke(GetSettingsForThisObject());
            OnExplode = delegate { };
            
            Destroy(gameObject);
        }

        private ExplosionSettings GetSettingsForThisObject()
        {
            return new ExplosionSettings
            {
                Position = GetPosition,
                Radius = RadiusOfEffect,
                Force = ForceAmount,
                DetonateOnRemote = !DetonateAutomatically
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
            return point;
        }

        [Serializable]
        public class ExplosionSettings
        {
            public Vector3 Position = Vector3.zero;
            public float Radius = 1f;
            public float Force = 1f;
            public bool DetonateOnRemote = false;
        }
    }
}
