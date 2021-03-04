using System.Linq;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using NaughtyAttributes;
using Tenshi.SaveHigan;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        [SerializeField] float cellSize;
        BoxCollider _collider;
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] bool enableGridGizmo = true;

        private void Awake()
        {
            SaveManager.LoadGameEvent += LoadGridFromFileAsync;
        }

        [Button(nameof(GenerateGrid), EButtonEnableMode.Editor)]
        private async void GenerateGrid()
        {
            obstacleMask = LayerMask.GetMask("Obstacle");
            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            grid = new Grid(new Node[x, y, z]);
            Vector3 centre = GetCentre;
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
                        var position = (new Vector3(xx, yy, zz) * cellSize) + centre;
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
            await SaveGridToFileAsync();
        }

        #region Saving & Loading

        const string GridSavePathName1 = "grid_1";
        const string GridSavePathName2 = "grid_2";
        const string GridSavePathName3 = "grid_3";
        const string GridSavePathName4 = "grid_4";
        const string GridInfoSaveName = "grid_Info";
        public event Action GridLoadedEvent;

        private async Task SaveGridToFileAsync()
        {
            //SAve here
            SerialisableVector gridDimensions = new SerialisableVector(new Vector3(x, y, x));

            var nodesToSave1 = new List<SerialisableNode>();
            var nodesToSave2 = new List<SerialisableNode>();
            var nodesToSave3 = new List<SerialisableNode>();
            var nodesToSave4 = new List<SerialisableNode>();
            int partition = x * y * z; partition /= 4;
            
            int i = 1;
            grid.LoopNode(node =>
            {
                var saveableNode = new SerialisableNode
                {
                    HasObstacle = node.HasObstacle,
                    Bounds = new SerialisableBounds(node.Bounds),
                    Position = new SerialisableVector(node.Position),
                    Index = new SerialisableVector(node.Index)
                };
                
                if (i > 0 && i <= (partition * 1))
                    nodesToSave1.Add(saveableNode);
                else if (i > (partition * 1) && i <= (partition * 2))
                    nodesToSave2.Add(saveableNode);
                else if (i > (partition * 2) && i <= (partition * 3))
                    nodesToSave3.Add(saveableNode);
                else if (i > (partition * 3) && i <= (partition * 4))
                    nodesToSave4.Add(saveableNode);

                i++;
            });

            $"Grid dimension info is X:{x}, Y:{y}, Z: {z}.".Msg();
            $"Total nodes present in grid should be {x*y*z}.".Msg();
            $"Detecting {nodesToSave1.Count + nodesToSave2.Count + nodesToSave3.Count} entries of nodes to save.".Msg();

            await SaveManager.SaveAsync(data: gridDimensions, pathKey: GridInfoSaveName);
            await SaveManager.SaveAsync(data: nodesToSave1, pathKey: GridSavePathName1);
            await SaveManager.SaveAsync(data: nodesToSave2, pathKey: GridSavePathName2);
            await SaveManager.SaveAsync(data: nodesToSave3, pathKey: GridSavePathName3);
            await SaveManager.SaveAsync(data: nodesToSave4, pathKey: GridSavePathName4);
        }

        private async void LoadGridFromFileAsync()
        {
            SaveManager.LoadGameEvent -= LoadGridFromFileAsync;
            await InternalLoadGridFromFileAsync();
        }

        private async Task InternalLoadGridFromFileAsync()
        {
            // setup
            grid = null;

            // load here
            var sVector = await SaveManager.LoadAsync<SerialisableVector>(pathKey: GridInfoSaveName);
            var sNodes1 = await SaveManager.LoadAsync<List<SerialisableNode>>(pathKey: GridSavePathName1);
            var sNodes2 = await SaveManager.LoadAsync<List<SerialisableNode>>(pathKey: GridSavePathName2);
            var sNodes3 = await SaveManager.LoadAsync<List<SerialisableNode>>(pathKey: GridSavePathName3);
            var sNodes4 = await SaveManager.LoadAsync<List<SerialisableNode>>(pathKey: GridSavePathName4);

            // deserialise here
            Vector3Int dimensions = TenshiConverter.DeserialiseVectorInt(sVector);
            Node[,,] nodeMultiArray = new Node[dimensions.x, dimensions.y, dimensions.z];
            await Task.Run(() =>
            {
                foreach (SerialisableNode sNode in sNodes1)
                {
                    Node node = new Node
                    {
                        HasObstacle = sNode.HasObstacle,
                        Bounds = TenshiConverter.DeserialiseBounds(sNode.Bounds),
                        Position = TenshiConverter.DeserialiseVector(sNode.Position),
                        Index = TenshiConverter.DeserialiseVectorInt(sNode.Index)
                    };
                    nodeMultiArray[node.Index.x, node.Index.y, node.Index.z] = node;
                }
                foreach (SerialisableNode sNode in sNodes2)
                {
                    Node node = new Node
                    {
                        HasObstacle = sNode.HasObstacle,
                        Bounds = TenshiConverter.DeserialiseBounds(sNode.Bounds),
                        Position = TenshiConverter.DeserialiseVector(sNode.Position),
                        Index = TenshiConverter.DeserialiseVectorInt(sNode.Index)
                    };
                    nodeMultiArray[node.Index.x, node.Index.y, node.Index.z] = node;
                }
                foreach (SerialisableNode sNode in sNodes3)
                {
                    Node node = new Node
                    {
                        HasObstacle = sNode.HasObstacle,
                        Bounds = TenshiConverter.DeserialiseBounds(sNode.Bounds),
                        Position = TenshiConverter.DeserialiseVector(sNode.Position),
                        Index = TenshiConverter.DeserialiseVectorInt(sNode.Index)
                    };
                    nodeMultiArray[node.Index.x, node.Index.y, node.Index.z] = node;
                }
                foreach (SerialisableNode sNode in sNodes4)
                {
                    Node node = new Node
                    {
                        HasObstacle = sNode.HasObstacle,
                        Bounds = TenshiConverter.DeserialiseBounds(sNode.Bounds),
                        Position = TenshiConverter.DeserialiseVector(sNode.Position),
                        Index = TenshiConverter.DeserialiseVectorInt(sNode.Index)
                    };
                    nodeMultiArray[node.Index.x, node.Index.y, node.Index.z] = node;
                }
            });

            // reconstruct grid for the game
            grid = new Grid(nodeMultiArray);

            if (pathFinder != null)
                pathFinder.SetGrid(grid);

            GridLoadedEvent?.Invoke();
        }

        #endregion

        #region Obstacles

        Mesh[] obstacleArray;

        void CheckObstacles()
        {
            int i = 1;
            grid.LoopNode(node =>
            {
                Bounds b = node.Bounds;
                foreach (var mesh in obstacleArray)
                {
                    Vector3[] vertices = mesh.vertices;
                    foreach (var vertex in vertices)
                    {
                        if (b.Contains(vertex))
                        {
                            if (node.HasObstacle)
                                continue;

                            node.HasObstacle = true;
                            $"Unique Obstacle Count: {i}".Msg();
                        }
                    }
                }

                // int meshIndex = -1;
                // while (++meshIndex < obstacleArray.Length)
                // {
                //     Vector3[] vertices = obstacleArray[meshIndex].vertices;

                //     unsafe
                //     {
                //         //! Usage of unsafe code to work with C++ pointers
                //         fixed (Vector3* vertexPointer = vertices)
                //         {
                //             int length = vertices.Length;
                //             Vector3* vertex = vertexPointer;

                //             while (length-- > 0)
                //             {
                //                 if (b.Contains(*vertex))
                //                 {
                //                     if (g.HasObstacle)
                //                         continue;

                //                     g.HasObstacle = true;
                //                     $"Unique Obstacle Count: {i}".Msg();
                //                 }
                //                 vertex++;
                //             }
                //         }
                //     }
                // }

                // obstacleArray(m =>
                // {
                //     Vector3[] vertices = m.vertices;


                //     if (m.bounds.Intersects(b))
                //     {
                //         g.HasObstacle = true;
                //         $"Obstacle Count: {i}".Msg();
                //         i++;
                //     }
                // });
            });
            obstacleArray = null;
        }

        private void GetAllObstaclesInScene()
        {
            if (obstacleArray.IsNullOrEmpty())
            {
                Bounds gridBound = new Bounds(GetCentre, GetSize);
                obstacleArray = FindObjectsOfType<MeshFilter>()
                    .Where(o => o.gameObject.layer == obstacleMask.ToLayer())
                    .Where(o => gridBound.Contains(o.transform.position))
                    .Select(o => o.mesh)
                    .ToArray();
            }
        }

        #endregion

        public Vector3 GetCentre => transform.position;
        public Vector3 GetSize => new Vector3(x, y, z) * cellSize;

        private void OnDrawGizmos()
        {
            //Visualization
            Gizmos.DrawWireCube(GetCentre, GetSize);
            Gizmos.color = Color.red;
            if (grid != null || grid?.Get != null)
            {
                enableGridGizmo = false;
            }

            if (enableGridGizmo)
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