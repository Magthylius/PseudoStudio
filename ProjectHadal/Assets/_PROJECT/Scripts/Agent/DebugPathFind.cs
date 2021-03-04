using System;
using System.Collections.Generic;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.AI
{
    public class DebugPathFind : MonoBehaviour
    {
        [SerializeField] bool drawPath = false;
        [SerializeField] PathFinder pathFinder;
        [SerializeField] Grid3DClass gridClass;

        [SerializeField] Transform start;
        [SerializeField] Transform end;
        List<Vector3> wayPoints;

        private void Awake()
        {
            wayPoints.Clear();
        }

        private async void CreatePath()
        {
            Stack<Node> fullPath = await pathFinder.FindAsync(start.position, end.position);
            int count = fullPath.Count;
            int i = -1;
            while (++i < count)
                wayPoints.Add(fullPath.Pop().Position);
        }

        private void Update()
        {
            if (wayPoints.IsNullOrEmpty() || !drawPath) return;

            try
            {
                for (int i = 0; i < wayPoints.Count; i += 2)
                {
                    Vector3 A = wayPoints[i];
                    Vector3 B;
                    if (i + 1 >= wayPoints.Count) break;
                    B = wayPoints[i + 1];

                    Debug.DrawLine(A, B, Color.blue);
                }
            }
            catch (Exception ex)
            {
                ex.Warn();
                drawPath = false;
            }
        }
    }
}
