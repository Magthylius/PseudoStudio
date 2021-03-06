using System;
using System.Linq;
using System.Collections.Generic;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.AI
{
    public class DebugPathFind : MonoBehaviour
    {
        [SerializeField] PathFinder pathFinder;
        [SerializeField] GridGenerator gridClass;
        [SerializeField] GameObject cubeTest;
        [SerializeField] LineRenderer line;
        [SerializeField] Transform start;
        [SerializeField] Transform end;
        int speed = 5;
        List<Vector3> wayPoints;
        Queue<Vector3> pathQueue;

        private void Awake()
        {
            wayPoints = new List<Vector3>();
            wayPoints.Clear();
            pathQueue = new Queue<Vector3>();
            line.enabled = false;
        }

        [Button("Debug Path Find")]
        private async void CreatePath()
        {
            pathFinder.SetGrid(gridClass.grid);
            Stack<Node> fullPath = await pathFinder.FindAsync(start.position, end.position);

            int count = fullPath.Count;
            $"Waypoint count: {count}".Msg();
            if (count == 0) return;
            
            wayPoints.Add(start.position);
            pathQueue.Enqueue(start.position);
            int i = -1;
            while (++i < count)
            {
                var temp = fullPath.Pop().Position;
                wayPoints.Add(temp);
                pathQueue.Enqueue(temp);
            }
            wayPoints.Add(end.position);
            pathQueue.Enqueue(end.position);

            isFirstPath = true;
            line.positionCount = wayPoints.Count;
            for (i = 0; i < wayPoints.Count; i++)
                line.SetPosition(i, wayPoints[i]);
            
            line.enabled = true;
        }

        private void Update()
        {
            if (wayPoints.IsNullOrEmpty()) return;

            if (isFirstPath)
            {
                isFirstPath = false;
                SetTarget(pathQueue.Dequeue());
            }

            if (Vector3.Distance(cubeTest.transform.position, curTarget).IsLessThan(0.01f))
            {
                if (pathQueue.Count == 0)
                {
                    wayPoints.Clear();
                    line.enabled = false;
                    return;
                }
                SetTarget(pathQueue.Dequeue());
            }

            Vector3 direction = (curTarget - cubeTest.transform.position).normalized;
            float multiplier = (Vector3.Distance(cubeTest.transform.position, end.position) + 1f);
            cubeTest.transform.position = Vector3.Lerp(cubeTest.transform.position, curTarget, multiplier * Time.deltaTime);
        }

        private bool isFirstPath = true;
        private Vector3 curTarget;

        void SetTarget(Vector3 t)
        {
            curTarget = t;
            TLog.Vector(t, "New target is");
        }

        void DrawDebugLine(Vector3 A, Vector3 B, Color color)
        {
            Debug.DrawLine(A, B, color, Mathf.Infinity);
        }
    }
}
