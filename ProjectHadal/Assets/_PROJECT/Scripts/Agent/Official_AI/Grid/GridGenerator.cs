using System.Linq;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;
using System;
using NaughtyAttributes;
using Tenshi.SaveHigan;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Hadal.AI.GeneratorGrid
{
    [ExecuteInEditMode]
    public class GridGenerator : MonoBehaviour
    {
        #region Variables

        [SerializeField] string projectResourcesPath;

        //! Reference grid class
        public Grid grid { get; private set; }
        //! X, Y, Z of the grid
        [SerializeField] int x, y, z;
        //! Cellsize of the grid
        [SerializeField] float cellSize;
        //! This is to help merge the node obstacles through the amount of obstacle nodes. 
        [SerializeField] float meshNodeSizeMultiplier;

        //! Layer mask for obstacle
        [SerializeField] LayerMask obstacleMask;

        //! Enable debug visuals
        [SerializeField] bool enableObstacleGridGizmo = true;
        [SerializeField] bool enableAllElseGridGizmo = false;
        //! Enable saving debug logs
        [SerializeField] bool enableSavingDebugLogs = false;
        [SerializeField] bool enablePathfindingGizmo = false;

        #endregion

        private void Awake()
        {
            if (!Application.isPlaying && Application.isEditor) return;

            grid = null;
#if !UNITY_EDITOR // Build
            projectResourcesPath = $"{Application.productName}_Data/Resources/";
#endif

            SaveManager.LoadGameEvent += LoadGridFromFileAsync; //! Load grid save file during play
        }

        private void Update()
        {
            if (Application.isPlaying && Application.isEditor) return;
            ScheduleObstacleCheckJob();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && Application.isEditor) return;
            CompleteObstacleCheckJob();
        }

        #region Grid Generator

        ///<summary> Generate 3D Grid </summary>
        [Button("Generate & Save Grid", EButtonEnableMode.Editor)]
        private async void GenerateGrid()
        {
            //! stopwatch
            Stopwatch watch = Stopwatch.StartNew();

            //! generate cancellation token
            token = new CancellationTokenSource();

            //! Setup obstacleMask & collider
            obstacleMask = LayerMask.GetMask("Obstacle");

            //! X, Y or Z must be Even number
            if (x.IsOdd()) x++;
            if (y.IsOdd()) y++;
            if (z.IsOdd()) z++;

            //! Init new grid
            grid = new Grid(new Node[x, y, z]);

            //! Loop with half length offset in order to set node positions in the scene according to Centre of grid
            int valX = 0, valY = 0, valZ = 0;
            int halfX = (int)(x * 0.5f);
            int halfY = (int)(y * 0.5f);
            int halfZ = (int)(z * 0.5f);
            //! 3D loop to generate the grid
            for (int xx = -halfX; xx < halfX; xx++)
            {
                if (valX >= grid.Get.GetLength(0))
                    valX--;

                valY = 0;
                for (int yy = -halfY; yy < halfY; yy++)
                {
                    if (valY >= grid.Get.GetLength(1))
                        valY--;

                    valZ = 0;
                    for (int zz = -halfZ; zz < halfZ; zz++)
                    {
                        if (valZ >= grid.Get.GetLength(2))
                            valZ--;

                        //! Create new node with configurations and assign to appropriate index in grid
                        var node = new Node();
                        var position = ((new Vector3(xx, yy, zz) * cellSize) + GetCentre) + (Vector3.one * 10f);
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

            //! Handle obstacle detection
            GetAllObstaclesInScene();
            await CheckObstacles();

            //! Save all generated grid data to file
            await SaveGridToFileAsync();

            //! end stopwatch
            watch.Stop();
            $"Elapsed time for Generate Grid: {watch.ElapsedMilliseconds}".Msg();
        }
        #endregion

        #region Saving & Loading

        /// <summary>The name of path to save </summary>
        const string GridSavePathName = "grid_";
        /// <summary>Used to load data, the list of nodes we have saved</summary>
        const string NodePartitionSaveName = "partition_Info";
        public static event Action<Grid> GridLoadedEvent;

        /// <summary>Save grid to a file</summary>
        private async Task SaveGridToFileAsync()
        {
            //Save here
            SerialisableVector gridDimensions = new SerialisableVector(new Vector3(x, y, z));

            //Save all the nodes
            List<List<SerialisableNode>> listOfNodesToSave = new List<List<SerialisableNode>>();

            //Amount of node count per partition(list)
            int maxPartitionSize = 5000000;

            //total size of the grid
            int totalSize = x * y * z;

            //amount required to save all nodes
            int partitionCount = Mathf.CeilToInt(totalSize / maxPartitionSize.AsFloat());

            //add the lists to the list of lists
            int i = -1;
            while (++i < partitionCount)
                listOfNodesToSave.Add(new List<SerialisableNode>());

            // Loop through all nodes and create a serialisable version to be stored in a list (this list will then be saved)
            int listIndex = 0;
            i = 1;
            grid.LoopNode(node =>
            {
                var saveableNode = new SerialisableNode
                {
                    HasObstacle = node.HasObstacle
                };

                if (i > maxPartitionSize)
                {
                    listIndex++;
                    i = 1;
                }
                listOfNodesToSave[listIndex].Add(saveableNode);

                i++;
            });

            //! Log save info predictions and resultant data
            if (enableSavingDebugLogs == true)
            {
                await $"Grid dimension info is X:{x}, Y:{y}, Z: {z}.".MsgAsync();
                await $"Total nodes present in grid should be {x * y * z}.".MsgAsync();
                await $"Partitions generated for this save: {partitionCount}.".MsgAsync();
                await $"Detecting {listOfNodesToSave.Sum(list => list.Count)} entries of nodes to save.".MsgAsync();
            }

            //! Save relevant data to files to resources
            SaveManager.SaveToResources(data: partitionCount, resourcePath: projectResourcesPath, subPathKey: NodePartitionSaveName);
            i = 1;
            foreach (var list in listOfNodesToSave)
            {
                await SaveManager.SaveToResourcesAsync(data: list, resourcePath: projectResourcesPath, subPathKey: $"{GridSavePathName}{i}");
                i++;
            }
        }

        /// <summary>Load Grid from saved file</summary>
        [Button(nameof(LoadGridFromFileAsync), EButtonEnableMode.Editor)]
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
            var sNodeListCount = await SaveManager.LoadFromResourcesAsync<int>(resourcePath: projectResourcesPath, subPathKey: NodePartitionSaveName);
            List<List<SerialisableNode>> sListOfSNodes = new List<List<SerialisableNode>>();
            int i = -1;
            while (++i < sNodeListCount)
                sListOfSNodes.Add(await SaveManager.LoadFromResourcesAsync<List<SerialisableNode>>(resourcePath: projectResourcesPath, subPathKey: $"{GridSavePathName}{i + 1}"));

            // deserialise data into nodes for the grid
            Node[,,] nodeMultiArray = new Node[x, y, z];
            Vector3 gridCentre = GetCentre;

            await Task.Run(() =>
            {
                Queue<Node> nodeQ = new Queue<Node>();

                foreach (List<SerialisableNode> sNodes in sListOfSNodes)
                {
                    foreach (SerialisableNode sNode in sNodes)
                    {
                        Node node = new Node { HasObstacle = sNode.HasObstacle };
                        nodeQ.Enqueue(node);
                    }
                }

                //! Loop with half length offset in order to set node positions in the scene according to Centre of grid
                int valX = 0, valY = 0, valZ = 0;
                int halfX = (int)(x * 0.5f);
                int halfY = (int)(y * 0.5f);
                int halfZ = (int)(z * 0.5f);
                //! 3D loop to generate the grid
                for (int xx = -halfX; xx < halfX; xx++)
                {
                    if (valX >= x)
                        valX--;

                    valY = 0;
                    for (int yy = -halfY; yy < halfY; yy++)
                    {
                        if (valY >= y)
                            valY--;

                        valZ = 0;
                        for (int zz = -halfZ; zz < halfZ; zz++)
                        {
                            if (valZ >= z)
                                valZ--;

                            //! Recreate dequeued node with configurations and assign to appropriate index in grid
                            var node = nodeQ.Dequeue();
                            var position = ((new Vector3(xx, yy, zz) * cellSize) + gridCentre) + (Vector3.one * 10f);
                            node.Position = position;
                            node.Bounds = new Bounds(position, new Vector3(cellSize, cellSize, cellSize));
                            node.Index = new Vector3Int(valX, valY, valZ);
                            nodeMultiArray[valX, valY, valZ] = node;

                            valZ++;
                        }
                        valY++;
                    }
                    valX++;
                }
            });

            // reconstruct grid for the game
            grid = new Grid(nodeMultiArray);

            GridLoadedEvent?.Invoke(grid);
        }

        #endregion

        #region Obstacles

        #region Obstacle Jobs

        bool enableObstacleJobs;
        JobHandle obstacleJobHandle;
        CheckObstaclesJob obstaclesJob;
        int obstacleJobPartitionsPerThread = 64;

        void ScheduleObstacleCheckJob()
        {
            if (!enableObstacleJobs) return;
            obstaclesJob = new CheckObstaclesJob(grid.Get.Length)
            {
                ObstacleInfos = ConvertObstaclesToNative(obstacleInfos),
                Nodes = ConvertNodesToNative(grid.GetAs1DArray())
            };

            obstacleJobHandle = obstaclesJob.Schedule(obstaclesJob.Nodes.Length, obstacleJobPartitionsPerThread);
        }
        void CompleteObstacleCheckJob()
        {
            if (!enableObstacleJobs) return;
            obstacleJobHandle.Complete();
            obstacleInfos = ConvertObstaclesToReference(obstaclesJob.ObstacleInfos);
            obstaclesJob.ObstacleInfos.NativeForEach(o => o.Dispose());
            $"Checking Obstacles... {obstaclesJob.CompletionRatio:F2}%".Msg();
        }

        NativeList<NativeObstacleInfo> ConvertObstaclesToNative(ObstacleInfo[] infos)
        {
            var list = new NativeList<NativeObstacleInfo>();
            foreach (var o in infos)
            {
                NativeObstacleInfo i = new NativeObstacleInfo { Filter = o.Filter, VertexBounds = o.VertexBounds.ToNativeList(Allocator.TempJob) };
                list.Add(i);
            }
            return list;
        }

        ObstacleInfo[] ConvertObstaclesToReference(NativeList<NativeObstacleInfo> infos)
        {
            var list = new List<ObstacleInfo>();
            foreach (var o in infos)
            {
                ObstacleInfo i = new ObstacleInfo { Filter = o.Filter, VertexBounds = o.VertexBounds.ToList() };
                list.Add(i);
            }
            return list.ToArray();
        }

        NativeList<NativeNode> ConvertNodesToNative(Node[] nodes)
        {
            var list = new NativeList<NativeNode>();
            foreach (var n in nodes)
            {
                NativeNode node = new NativeNode { Bounds = n.Bounds, HasObstacle = n.HasObstacle, Index = n.Index };
                list.Add(node);
            }
            return list;
        }

        Node[] ConvertNodesToReference(NativeList<NativeNode> nodes)
        {
            var list = new List<Node>();
            foreach (var n in nodes)
            {
                Node node = new Node { Bounds = n.Bounds, HasObstacle = n.HasObstacle, Index = n.Index };
                list.Add(node);
            }
            return list.ToArray();
        }

        struct NativeNode
        {
            public Bounds Bounds { get; set; }
            public bool HasObstacle { get; set; }
            public Vector3Int Index { get; set; }
            public bool IsNull => Bounds == null;
        }

        struct NativeObstacleInfo
        {
            public MeshFilter Filter { get; set; }
            public NativeList<Bounds> VertexBounds { get; set; }
            public bool IsNull => Filter == null;
            public void Dispose() => VertexBounds.Dispose();
        }

        [BurstCompile]
        struct CheckObstaclesJob : IJobParallelFor
        {
            public NativeList<NativeObstacleInfo> ObstacleInfos;
            public NativeList<NativeNode> Nodes;
            public float CompletionRatio;
            private float MaxCount;
            private int ElapsedCount;

            public void Execute(int i)
            {
                Nodes[i] = HandleNodesToObstacleComparison(Nodes[i], 100f * (ElapsedCount++ / MaxCount));
            }

            NativeNode HandleNodesToObstacleComparison(NativeNode node, float completionRatio)
            {
                if (node.IsNull) return node;

                CompletionRatio = completionRatio;

                if (ObstacleInfos.Length == 0)
                    return node;

                Bounds b = node.Bounds;
                int length = ObstacleInfos.Length;
                for (int j = 0; j < length; j++)
                {
                    var col = ObstacleInfos[j];
                    if (col.IsNull)
                        continue;

                    foreach (var m in col.VertexBounds)
                    {
                        if (b.Intersects(m))
                        {
                            if (node.HasObstacle) return node;
                            node.HasObstacle = true;
                            return node;
                        }
                    }
                }
                return node;
            }

            public CheckObstaclesJob(int maxCount)
            {
                ObstacleInfos = new NativeList<NativeObstacleInfo>();
                Nodes = new NativeList<NativeNode>();
                CompletionRatio = 0f;
                ElapsedCount = 1;
                MaxCount = maxCount;
            }
        }

        #endregion

        struct ObstacleInfo
        {
            public MeshFilter Filter { get; set; }
            public List<Bounds> VertexBounds { get; set; }

            public bool IsNull() => Filter == null;
        }

        ObstacleInfo[] obstacleInfos;

        CancellationTokenSource token;
        [Button(nameof(CancelGenerateGridTask))] void CancelGenerateGridTask() => token?.Cancel();

        async Task HandleNodesToObstacleComparison(Node node, float completionRatio)
        {
            if (node == null) return;

            await $"Checking Obstacles... {completionRatio:F2}%".MsgAsync();
            token?.Token.ThrowIfCancellationRequested();

            Bounds b = node.Bounds;
            if (obstacleInfos.IsNullOrEmpty())
                return;

            await Task.Run(() =>
            {
                int length = obstacleInfos.Length;
                for (int j = 0; j < length; j++)
                {
                    var col = obstacleInfos[j];
                    if (col.IsNull())
                        continue;

                    List<Bounds> meshBounds = new List<Bounds>(col.VertexBounds);
                    foreach (var m in meshBounds)
                    {
                        if (b.Intersects(m))
                        {
                            if (node.HasObstacle) return;
                            node.HasObstacle = true;
                            return;
                        }
                    }
                    meshBounds.Clear();
                }
            });
        }

        /// <summary>
        /// Check obstacles that collide with each node's bounds.</summary>
        async Task CheckObstacles()
        {
            await "Preparing to check obstacles...".MsgAsync();

            int i = 1;
            int totalNodes = grid.Get.Length;
            await grid.LoopAs1DArray_XNodesPerIterationAsync(async (nodes) =>
            {
                int c = -1;
                while (++c < nodes.Length)
                    await HandleNodesToObstacleComparison(nodes[c], 100f * (i++ / totalNodes.AsFloat()));

            }, token.Token, 10);

            #region Temp
            // await Task.Run(async () =>
            // {
            //     for (int x = 0; x < grid.Get.GetLength(0); x++)
            //     {
            //         for (int y = 0; y < grid.Get.GetLength(1); y++)
            //         {
            //             for (int z = 0; z < grid.Get.GetLength(2); z++)
            //             {
            //                 Node node = grid.GetNodeAt(new Vector3Int(x, y, z));

            //                 await "Checking Obstacles...".MsgAsync();
            //                 token?.Token.ThrowIfCancellationRequested();

            //                 Bounds b = node.Bounds;
            //                 if (obstacleInfos.IsNullOrEmpty())
            //                     return;

            //                 int length = obstacleInfos.Length;
            //                 bool shouldBreak = false;
            //                 for (int j = 0; j < length; j++)
            //                 {
            //                     var col = obstacleInfos[j];
            //                     if (col.IsNull())
            //                         continue;

            //                     List<Bounds> meshBounds = new List<Bounds>(col.VertexBounds);
            //                     foreach (var m in meshBounds)
            //                     {
            //                         if (b.Intersects(m))
            //                         {
            //                             if (node.HasObstacle) return;
            //                             node.HasObstacle = true;
            //                             //await UpdateObstacleCheckLoadingBar(meshBounds.Count);

            //                             shouldBreak = true;
            //                             break;
            //                         }
            //                     }
            //                     meshBounds.Clear();
            //                     if (shouldBreak)
            //                         break;
            //                 }
            //             }
            //         }
            //     }
            // });

            #endregion

            // grid.LoopNode(node => await DoSomething(node));

            obstacleInfos = new ObstacleInfo[obstacleInfos.Length];
        }

        ///<summary>Get all the objects that are under obstacle layer in the grid.</summary>
        private void GetAllObstaclesInScene()
        {
            Bounds gridBound = new Bounds(GetCentre, GetSize);

            var obstacleArray = FindObjectsOfType<MeshFilter>()                   // Find all objects of type MeshFilter in scene
                .Where(o => o.gameObject.layer == obstacleMask.ToLayer())   // Determine if layer is obstacle layer
                                                                            // .Where(o => gridBound.Contains(o.transform.position))                 
                .Where(o => o.GetComponent<Collider>().bounds.Intersects(gridBound))    // If the obstacles is inside the grid
                .ToArray();                                                 // Store in array

            obstacleInfos = new ObstacleInfo[obstacleArray.Length];
            for (int i = 0; i < obstacleArray.Length; i++)
            {
                obstacleInfos[i] = new ObstacleInfo
                {
                    Filter = obstacleArray[i],
                    VertexBounds = GetNodesFromMeshFilter(obstacleArray[i])
                };
            }

            $"Generated obstacle infos: {obstacleInfos.Length}".Msg();
        }

        private List<Bounds> GetNodesFromMeshFilter(MeshFilter meshFilter)
        {
            List<Bounds> points = new List<Bounds>();
            List<Vector3> vertices = new List<Vector3>();
            meshFilter.sharedMesh.GetVertices(vertices);

            foreach (var v in vertices)
            {
                var position = (meshFilter.transform.rotation * Vector3.Scale(v, meshFilter.transform.localScale)) + meshFilter.transform.position;
                points.Add(new Bounds(position, Vector3.one * (cellSize * meshNodeSizeMultiplier)));
            }
            return points;
        }

        #endregion

        #region Public getters
        /// <summary> Get center of the transform.position </summary>
        public Vector3 GetCentre => transform.position;
        /// <summary> Get the size of the grid </summary>
        public Vector3 GetSize => new Vector3(x, y, z) * cellSize;
        #endregion

        #region Debugging visuals 
        private void OnDrawGizmos()
        {
            //Visualization
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetCentre, GetSize);

            //! grid null check
            if (grid == null)
            {
                enableObstacleGridGizmo = false;
                enableAllElseGridGizmo = false;
                enablePathfindingGizmo = false;
            }

            //! visuals for obstacles nodes
            if (enableObstacleGridGizmo || enableObstacleGridGizmo && Application.isPlaying)
            {
                Gizmos.color = Color.white;
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

            //! visuals for non-obstacles nodes
            if (enableAllElseGridGizmo || enableAllElseGridGizmo && Application.isPlaying)
            {
                Gizmos.color = Color.black;
                grid.LoopNode(node =>
                {
                    if (node.HasObstacle) return;
                    Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                });
            }

            if (enablePathfindingGizmo || enablePathfindingGizmo && Application.isPlaying)
            {
                grid.LoopNode(node =>
                {
                    if (node.IsStart)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                        return;
                    }
                    else if (node.IsEnd)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                        return;
                    }

                    if (node.IsVisited && !node.IsPath)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                    }
                    else if (node.IsVisited && node.IsPath)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                    }
                });
            }
        }

        /// <summary> Draw debug bounds </summary>
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
        #endregion
    }

    static class JobNativeExtens
    {
        public static void NativeForEach<T>(this IEnumerable<T> list, Action<T> method) where T : struct
        {
            foreach (var i in list)
                method.Invoke(i);
        }
    }
}