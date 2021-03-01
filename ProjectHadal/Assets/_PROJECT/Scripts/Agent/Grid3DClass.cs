using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using NaughtyAttributes;

namespace Hadal.AI
{
    [RequireComponent(typeof(BoxCollider))]
    public class Grid3DClass : MonoBehaviour
    {
        private Node[,,] grid;
        [SerializeField] Vector3 dimensions;
        [SerializeField] int x;
        [SerializeField] int y;
        [SerializeField] int z;
        [SerializeField, Tenshi.ReadOnly] Vector3 centre = Vector3.zero;
        [SerializeField] float cellSize;
        BoxCollider _collider;
        [SerializeField] List<GameObject> obstacleList;
        [SerializeField] LayerMask obstacleMask;
        private bool enableGridGizmo = false;

        private void Awake()
        {
            _collider = gameObject.GetComponent<BoxCollider>();
            grid = new Node[x, y, z];
            var nodePrefab = Resources.Load(PathManager.GridNodePrefabPath);
            
            int valX = 0, valY = 0, valZ = 0;
            int halfX = (int)(grid.GetLength(0) * 0.5f);
            int halfY = (int)(grid.GetLength(1) * 0.5f);
            int halfZ = (int)(grid.GetLength(2) * 0.5f);
            for (int xx = -halfX; xx <= halfX; xx++)
            {
                if (valX >= grid.GetLength(0)) valX--;
                valY = 0;
                for (int yy = -halfY; yy <= halfY; yy++)
                {
                    if (valY >= grid.GetLength(1)) valY--;
                    valZ = 0;
                    for (int zz = -halfZ; zz <= halfZ; zz++)
                    {
                        if (valZ >= grid.GetLength(2)) valZ--;
                        grid[valX, valY, valZ] = new Node();
                        var position = (new Vector3(xx, yy, zz) * cellSize) + GetCentre;
                        grid[valX, valY, valZ].GObject = Instantiate(nodePrefab, position, Quaternion.identity, transform).AsGObject();
                        grid[valX, valY, valZ].GObject.name += $" {valX},{valY},{valZ}";
                        valZ++;
                    }
                    valY++;
                }
                valX++;
            }
        }

        private void Update()
        {
            CheckObstacles();
            Draw();
        }

        private void LoopGrid(Action<int, int, int> method)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        method.Invoke(x, y, z);
                    }
                }
            }
        }

        private void LoopGridOffset(Action<int, int, int> method)
        {
            int halfX = (int)(grid.GetLength(0) * 0.5f);    
            int halfY = (int)(grid.GetLength(1) * 0.5f);    
            int halfZ = (int)(grid.GetLength(2) * 0.5f);    
            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    for (int z = -halfZ; z <= halfZ; z++)
                    {
                        method.Invoke(x, y, z);
                    }
                }
            }
        }

        void Draw()
        {
            
        }

        void CheckObstacles()
        {
            for (int i = 0; i < obstacleList.Count; i++)
            {
                if(_collider.bounds.Contains(obstacleList[i].transform.position))
                {
                    "Obstacle".Msg();
                }
            }
        }

        [Button(nameof(GetAllObstaclesInScene))]
        private void GetAllObstaclesInScene()
        {
            if (obstacleList.IsNullOrEmpty())
            {
                obstacleList = FindObjectsOfType<GameObject>().Where(o => o.layer == obstacleMask.ToLayer()).ToList();
            }
        }

        public Vector3 GetCentre => transform.position;
        public Vector3 GetSize => new Vector3(x,y,z) * cellSize;

        private void OnDrawGizmos()
        {
            //Visualization
            Gizmos.DrawWireCube(GetCentre, GetSize);
            
            if (Application.isPlaying && enableGridGizmo)
            {
                LoopGridOffset((x, y, z) =>
                {
                    if (x.IsPrime() || y.IsPrime() || z.IsPrime())
                    {
                        var position = new Vector3(x, y, z) * cellSize;
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(position + GetCentre, new Vector3(cellSize,cellSize,cellSize));
                    }
                });
            }
        }

        [Button(nameof(ToggleEnableGridGizmo))]
        private void ToggleEnableGridGizmo() => enableGridGizmo = !enableGridGizmo;

        void DrawBounds(Bounds b, float delay = 0)
        {
            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, Color.blue, delay);
            Debug.DrawLine(p2, p3, Color.red, delay);
            Debug.DrawLine(p3, p4, Color.yellow, delay);
            Debug.DrawLine(p4, p1, Color.magenta, delay);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, Color.blue, delay);
            Debug.DrawLine(p6, p7, Color.red, delay);
            Debug.DrawLine(p7, p8, Color.yellow, delay);
            Debug.DrawLine(p8, p5, Color.magenta, delay);

            // sides
            Debug.DrawLine(p1, p5, Color.white, delay);
            Debug.DrawLine(p2, p6, Color.gray, delay);
            Debug.DrawLine(p3, p7, Color.green, delay);
            Debug.DrawLine(p4, p8, Color.cyan, delay);
        }
    }

    public class Node
    {
        public Vector3 Index { get; set; }
        public GameObject GObject { get; set; } = null;
        public Node() { }
    }
}