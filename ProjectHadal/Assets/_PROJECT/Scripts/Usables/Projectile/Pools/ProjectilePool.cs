using Tenshi.UnitySoku;
using NaughtyAttributes;
using UnityEngine;
using Hadal.Networking.UI.Loading;
using System.Collections.Generic;
using Tenshi;
using ReadOnlyAttribute = Tenshi.ReadOnlyAttribute;
//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class ProjectilePool<T> : ObjectPool<T> where T : ProjectileBehaviour
    {
        [ReadOnly, SerializeField] protected ProjectileData data;

        protected override void Start()
        {
            prefab = data.ProjectilePrefab.GetComponent<T>();
            InitialisationCompleted += AssignProjID;
            base.Start();
            LoadingManager.Instance.CheckInObjectPool();
        }

        #region Private Function
        private void AssignProjID(Queue<T> projectileBehaviors)
        {
            for(int i = 0; i < projectileBehaviors.Count; i++)
            {
                var projectileBehavior = projectileBehaviors.Requeue();
                projectileBehavior.projID += projectileBehavior.Data.ProjIDInt + (i + 1);
            }
        }
        #endregion
    }
}