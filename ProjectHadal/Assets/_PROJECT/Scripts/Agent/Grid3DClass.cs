using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using NaughtyAttributes;

namespace Hadal.AI
{
    public class Grid
    {
        public Grid(Node[,,] grid) => this.grid = grid;
        public Node[,,] grid;
        public Node[,,] Get => grid;
        public Node GetNodeAt(Vector3Int pos) => Get[pos.x, pos.y, pos.z];

        public void Loop(Action<int, int, int> method)
        {
            for (int x = 0; x < Get.GetLength(0); x++)
            {
                for (int y = 0; y < Get.GetLength(1); y++)
                {
                    for (int z = 0; z < Get.GetLength(2); z++)
                    {
                        method.Invoke(x, y, z);
                    }
                }
            }
        }
        public void LoopNode(Action<Node> method)
        {
            for (int x = 0; x < Get.GetLength(0); x++)
            {
                for (int y = 0; y < Get.GetLength(1); y++)
                {
                    for (int z = 0; z < Get.GetLength(2); z++)
                    {
                        method.Invoke(Get[x, y, z]);
                    }
                }
            }
        }
        public void LoopFull(Action<int, int, int, Node> method)
        {
            for (int x = 0; x < Get.GetLength(0); x++)
            {
                for (int y = 0; y < Get.GetLength(1); y++)
                {
                    for (int z = 0; z < Get.GetLength(2); z++)
                    {
                        method.Invoke(x, y, z, Get[x, y, z]);
                    }
                }
            }
        }
        public void LoopNode_Breakable(Action<Node> method, Func<bool> shouldBreak)
        {
            for (int x = 0; x < Get.GetLength(0); x++)
            {
                for (int y = 0; y < Get.GetLength(1); y++)
                {
                    for (int z = 0; z < Get.GetLength(2); z++)
                    {
                        method.Invoke(Get[x, y, z]);
                        if (shouldBreak.Invoke())
                            break;
                    }
                }
            }
        }
    }

    public class Grid3DClass : MonoBehaviour
    {
        private Grid grid;
        [SerializeField] PathFinder pathFinder;
        [SerializeField] Vector3 dimensions;
        [SerializeField] int x;
        [SerializeField] int y;
        [SerializeField] int z;
        [SerializeField, Tenshi.ReadOnly] Vector3 centre = Vector3.zero;
        [SerializeField] float cellSize;
        BoxCollider _collider;
        [SerializeField] List<Collider> obstacleList;
        [SerializeField] LayerMask obstacleMask;  
        [SerializeField] bool enableGridGizmo = true;

        private void Awake()
        {
            obstacleMask = LayerMask.GetMask("Obstacle");
            //Get box collider
            _collider = gameObject.GetComponent<BoxCollider>();
            grid = new Grid(new Node[x, y, z]);
            int valX = 0, valY = 0, valZ = 0;
            int halfX = (int)(grid.Get.GetLength(0) * 0.5f);
            int halfY = (int)(grid.Get.GetLength(1) * 0.5f);
            int halfZ = (int)(grid.Get.GetLength(2) * 0.5f);
            for (int xx = -halfX; xx <= halfX; xx++)
            {
                if (valX >= grid.Get.GetLength(0))
                    valX--;

                valY = 0;
                for (int yy = -halfY; yy <= halfY; yy++)
                {
                    if (valY >= grid.Get.GetLength(1))
                        valY--;

                    valZ = 0;
                    for (int zz = -halfZ; zz <= halfZ; zz++)
                    {
                        if (valZ >= grid.Get.GetLength(2))
                            valZ--;

                        var node = new Node();
                        var position = (new Vector3(xx, yy, zz) * cellSize) + GetCentre;
                        node.Position = position;
                        node.Bounds = new Bounds(position, new Vector3(cellSize, cellSize, cellSize));
                        node.HasObstacle = false;
                        node.Index = new Vector3Int(valX, valY, valZ);
                        grid.Get[valX, valY, valZ] = node;
                        valZ++;
                    }
                    valY++;
                }
                valX++;
            }

            GetAllObstaclesInScene();
            CheckObstacles();
            if (pathFinder != null)
                pathFinder.SetGrid(grid);
        }

        private void Update()
        {

        }

        void CheckObstacles()
        {
            int i = 1;
            grid.LoopNode(node =>
            {
                Node g = node;
                Bounds b = g.Bounds;
                obstacleList.ForEach(o =>
                {
                    if (o.bounds.Intersects(b))
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
                Bounds gridBound = new Bounds(GetCentre, GetSize);
                obstacleList = FindObjectsOfType<Collider>()
                    .Where(o => o.gameObject.layer == obstacleMask.ToLayer())
                    .Where(o => gridBound.Contains(o.transform.position))
                    .ToList();
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
                grid.LoopNode(node =>
                {
                    var g = node;
                    var b = g.Bounds;
                    if (g.HasObstacle)
                    {
                        Gizmos.DrawWireCube(b.center, b.size);
                    }
                });
            }
        }

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
}