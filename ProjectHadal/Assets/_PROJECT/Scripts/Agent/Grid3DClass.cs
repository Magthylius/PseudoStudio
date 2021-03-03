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
        [SerializeField] List<Collider> obstacleList;
        [SerializeField] LayerMask obstacleMask;  
        private bool enableGridGizmo = true;

        private void Awake()
        {
            obstacleMask = LayerMask.GetMask("Obstacle");
            //Get box collider
            _collider = gameObject.GetComponent<BoxCollider>();
            var nodePrefab = Resources.Load(PathManager.GridNodePrefabPath);
            grid = new Node[x, y, z];
            int valX = 0, valY = 0, valZ = 0;
            int halfX = (int)(grid.GetLength(0) * 0.5f);
            int halfY = (int)(grid.GetLength(1) * 0.5f);
            int halfZ = (int)(grid.GetLength(2) * 0.5f);
            for (int xx = -halfX; xx <= halfX; xx++)
            {
                if (valX >= grid.GetLength(0))
                    valX--;

                valY = 0;
                for (int yy = -halfY; yy <= halfY; yy++)
                {
                    if (valY >= grid.GetLength(1))
                        valY--;

                    valZ = 0;
                    for (int zz = -halfZ; zz <= halfZ; zz++)
                    {
                        if (valZ >= grid.GetLength(2))
                            valZ--;

                        var node = new Node();
                        var position = (new Vector3(xx, yy, zz) * cellSize) + GetCentre;
                        node.Position = position;
                        node.Bounds = new Bounds(position, new Vector3(cellSize, cellSize, cellSize));
                        node.HasObstacle = false;
                        grid[valX, valY, valZ] = node;
                        valZ++;
                    }
                    valY++;
                }
                valX++;
            }

            GetAllObstaclesInScene();
            CheckObstacles();
        }

        private void Update()
        {

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

        void CheckObstacles()
        {
            int i = 1;
            LoopGrid((x, y, z) =>
            {
                Node g = grid[x, y, z];
                Bounds b = g.Bounds;
                obstacleList.ForEach(o =>
                {
                    if (b.Intersects(o.bounds))
                    {
                        g.HasObstacle = true;
                        $"Obstacle Count: {i}".Msg();
                        i++;
                    }
                });
            });
            obstacleList.Clear();
        }
        
        private void GetAllObstaclesInScene()
        {
            if (obstacleList.IsNullOrEmpty())
            {
                obstacleList = FindObjectsOfType<Collider>().Where(o => o.gameObject.layer == obstacleMask.ToLayer()).ToList();
            }
        }

        public Vector3 GetCentre => transform.position;
        public Vector3 GetSize => new Vector3(x,y,z) * cellSize;

        private void OnDrawGizmos()
        {
            //Visualization
            Gizmos.DrawWireCube(GetCentre, GetSize);
            Gizmos.color = Color.red;
            if (Application.isPlaying && enableGridGizmo)
            {
                LoopGrid((x, y, z) =>
                {
                    var g = grid[x, y, z];
                    var b = g.Bounds;
                    if (g.HasObstacle)
                    {
                        Gizmos.DrawWireCube(b.center, b.size);
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
        public bool HasObstacle { get; set; } = false;
        public Bounds Bounds { get; set; }
        public Vector3 Position { get; set; }
        public Node() { }
    }
}