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
    public class GridGenerator : MonoBehaviour
    {
        #region Variables
        //! Reference grid class
        public Grid grid { get; private set; }
        //! Reference pathfinder class
        [SerializeField] PathFinder pathFinder;
        //! X, Y, Z of the grid
        [SerializeField] int x, y, z;
        //! Cellsize of the grid
        [SerializeField] float cellSize;

        //! Layer mask for obstacle
        [SerializeField] LayerMask obstacleMask;

        //! Enable debug visuals
        [SerializeField] bool enableObstacleGridGizmo = true;
        [SerializeField] bool enableAllElseGridGizmo = false;
        //! Enable saving debug logs
        [SerializeField] bool enableSavingDebugLogs = false;

        //! These two list is to separate obstacles and non obstacles contained nodes.
        public List<Node> emptyNodeList { get; private set; }
        List<Node> obstacleNodeList;
        #endregion

        private void Awake()
        {
            grid = null; //! Init null
            SaveManager.LoadGameEvent += LoadGridFromFileAsync; //! Load grid save file during play
        }

        #region Grid Generator

        ///<summary> Generate 3D Grid </summary>
        [Button("Generate & Save Grid", EButtonEnableMode.Editor)]
        private async void GenerateGrid()
        {
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
                        var position = ((new Vector3(xx, yy, zz) * cellSize) + GetCentre) + Vector3.one;
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
        }
        #endregion

        #region Saving & Loading

        /// <summary>The name of path to save </summary>
        const string GridSavePathName = "grid_";
        /// <summary>Save X, Y, Z dimension of the grid </summary>
        const string GridInfoSaveName = "grid_Info";
        /// <summary>Used to load data, the list of nodes we have saved</summary>
        const string NodePartitionSaveName = "parition_Info";
        public event Action<Grid> GridLoadedEvent;

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
                    HasObstacle = node.HasObstacle,
                    Bounds = new SerialisableBounds(node.Bounds),
                    Position = new SerialisableVector(node.Position),
                    Index = new SerialisableVector(node.Index)
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


            //! Save relevant data to files
            SaveManager.Save(data: partitionCount, pathKey: NodePartitionSaveName);
            await SaveManager.SaveAsync(data: gridDimensions, pathKey: GridInfoSaveName);
            i = 1;
            foreach (var list in listOfNodesToSave)
            {
                await SaveManager.SaveAsync(data: list, pathKey: $"{GridSavePathName}{i}");
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
            var sVector = await SaveManager.LoadAsync<SerialisableVector>(pathKey: GridInfoSaveName);
            var sNodeListCount = await SaveManager.LoadAsync<int>(pathKey: NodePartitionSaveName);
            List<List<SerialisableNode>> sListOfSNodes = new List<List<SerialisableNode>>();
            int i = -1;
            while (++i < sNodeListCount)
                sListOfSNodes.Add(await SaveManager.LoadAsync<List<SerialisableNode>>(pathKey: $"{GridSavePathName}{i + 1}"));

            // deserialise data into nodes for the grid
            Vector3Int dimensions = TenshiConverter.DeserialiseVectorInt(sVector);
            Node[,,] nodeMultiArray = new Node[dimensions.x, dimensions.y, dimensions.z];
            await Task.Run(() =>
            {
                foreach (List<SerialisableNode> sNodes in sListOfSNodes)
                {
                    foreach (SerialisableNode sNode in sNodes)
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
                }
            });

            // reconstruct grid for the game
            grid = new Grid(nodeMultiArray);
            x = dimensions.x;
            y = dimensions.y;
            z = dimensions.z;

            if (pathFinder != null)
                pathFinder.SetGrid(grid);

            GridLoadedEvent?.Invoke(grid);
        }

        #endregion

        #region Obstacles

        Collider[] obstacleArray;
        /// <summary>
        /// Check obstacles that collide with each node's bounds and add it to a obstacleNodeList. Else, add to emptyNodeList</summary>
        async Task CheckObstacles()
        {
            await 1.MsgAsync();
            int i = 1;

            obstacleNodeList = new List<Node>();
            emptyNodeList = new List<Node>();

            grid.LoopNode((node) =>
            {
                2.Msg();

                Bounds b = node.Bounds;
                if (obstacleArray.IsNullOrEmpty())
                {
                    $"Empty!".Msg();
                    return;
                }

                int length = obstacleArray.Length;
                for (int j = 0; j < length; j++)
                {
                    3.Msg();

                    var col = obstacleArray[j];
                    if (col == null)
                    {
                        $"Collider is null".Msg();
                        continue;
                    }

                    if (b.Intersects(col.bounds))
                    {
                        4.Msg();
                        $"Unique Obstacle Count: {i}".Msg();
                        node.HasObstacle = true;
                        obstacleNodeList.Add(node);
                        i++;
                        return;
                    }
                    else
                    {
                        emptyNodeList.Add(node);
                    }
                }

                // TODO: Check with vertices instead of AABB.
                #region Unused

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

                #endregion
            });

            if (enableSavingDebugLogs)
            {
                $"Obstacle Nodes: {obstacleNodeList.Count}".Msg();
                $"Empty Nodes: {emptyNodeList.Count}".Msg();
            }
            obstacleArray = null;
        }

        ///<summary>Get all the objects that are under obstacle layer in the grid.</summary>
        private void GetAllObstaclesInScene()
        {
            Bounds gridBound = new Bounds(GetCentre, GetSize);

            obstacleArray = FindObjectsOfType<Collider>()                   // Find all objects of type MeshFilter in scene
                .Where(o => o.gameObject.layer == obstacleMask.ToLayer())   // Determine if layer is obstacle layer
                .Where(o => gridBound.Intersects(o.bounds))                 // If the obstacles is inside the grid
                .ToArray();                                                 // Store in array
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
            Gizmos.DrawWireCube(GetCentre, GetSize);

            //! grid null check
            if (grid == null)
            {
                enableObstacleGridGizmo = false;
                enableAllElseGridGizmo = false;
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
}