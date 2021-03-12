using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;
using Hadal.AI.GeneratorGrid;

namespace Hadal.AI.AStarPathfinding
{
    public class PathFinder : Singleton<PathFinder>
    {
        [Header("Tweak maxBound for pathfinding.")]
        [SerializeField] int maxBound;
        public int _MaxBound { get => maxBound; }
        Grid grid;
        Stopwatch watch;

        protected override void Awake()
        {
            base.Awake();
            GridGenerator.GridLoadedEvent += SetGrid;
        }

        void OnDestroy() => GridGenerator.GridLoadedEvent -= SetGrid;

        public void SetGrid(Grid grid) => this.grid = grid;

        public Stack<Node> Find(Vector3 from, Vector3 to)
        {
            return Find(GetNodePosition(from, "start"), GetNodePosition(to, "end"));
        }

        public Stack<Node> Find(Node from, Node to)
        {
            watch = Stopwatch.StartNew();

            //! Setup for A*
            Node theStart = from;

            Node theEnd = to;
            if (from == null || to == null)
                return new Stack<Node>();

            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();
            List<Node> neighbours;
            Node current = theStart;
            open.Add(theStart);

            //! Setup node costs
            grid.LoopNode(node => node.ResetPathfindingInfo());
            theStart.GCost = 0;
            theStart.FCost = GetHeuristic(theStart, theStart);

            //! Run A*
            while (open.IsNotEmpty())
            {
                current = open.First();
                if (current == theEnd)
                    return ReconstructPath(current, theStart);

                open.Remove(current);
                closed.Add(current);
                neighbours = GetNeighbours(current);

                foreach (Node n in neighbours)
                {
                    if (NodeIsNeverEvaluated(n) && !n.HasObstacle)
                    {
                        n.Parent = current;
                        n.GCost = current.GCost + GetAppendedGCost(n, current);
                        n.FCost = n.GCost + GetHeuristic(n, theEnd);
                        open.Add(n);
                        open = open.OrderBy(node => node.FCost).ToList();
                    }
                }
                neighbours.Clear();
            }

            //! Invalid / Failed Path case
            return new Stack<Node>();

            #region Local Methods

            bool NodeIsNeverEvaluated(Node n) => !closed.Contains(n) && !open.Contains(n);

            #endregion
        }

        public async Task<Stack<Node>> FindAsync(Vector3 from, Vector3 to)
        {
            return await FindAsync(GetNodePosition(from, "start"), GetNodePosition(to, "end"));
        }

        /// <summary>
        /// Do A* and find the path from start to end
        /// </summary>
        /// <param name="from">The AI starting position</param>
        /// <param name="to">the ending position</param>
        public async Task<Stack<Node>> FindAsync(Node from, Node to)
        {
            watch = Stopwatch.StartNew();

            //! Setup for A*
            float weight = 1f;

            Node theStart = from;
            Node theEnd = to;
            if (from == null || to == null)
                return new Stack<Node>();

            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();
            List<Node> neighbours;
            Node current = theStart;
            open.Add(theStart);

            //! Setup node costs
            grid.LoopNode(node => node.ResetPathfindingInfo());
            theStart.GCost = 0;
            theStart.FCost = theStart.GCost + GetHeuristic(theStart, theEnd);
            theStart.IsStart = true;
            theEnd.IsEnd = true;

            //! Run A*
            while (open.IsNotEmpty())
            {
                await "A Star is running...".MsgAsync();

                current = open.OrderBy(node => node.FCost).First();
                current.IsVisited = true;
                if (current == theEnd)
                {
                    return await ReconstructPathAsync(current, theStart);
                }

                open.Remove(current);
                closed.Add(current);
                neighbours = GetNeighbours(current);

                await Task.Run(async () =>
                {
                    //await 1.MsgAsync();
                    foreach (Node n in neighbours)
                    {
                        //await 2.MsgAsync();
                        if (n.IsVisited || n.HasObstacle)
                            continue;

                        //await 3.MsgAsync();

                        float potentialCost = current.GCost + GetAppendedGCost(n, current);
                        if (potentialCost < n.GCost)
                        {
                            //await 4.MsgAsync();
                            n.Parent = current;
                            n.GCost = potentialCost;
                            n.FCost = n.GCost + (weight * GetHeuristic(n, theEnd));
                            
                            if (!open.Contains(n))
                            {
                                //await 5.MsgAsync();
                                open.Add(n);
                            }
                        }

                        
                    }
                });
                neighbours.Clear();
            }

            //! Invalid / Failed Path case
            return new Stack<Node>();

            #region Local Methods

            bool NodeIsNeverEvaluated(Node n) => !closed.Contains(n) && !open.Contains(n);

            #endregion
        }

        /// <summary> We can customise how the G cost is calculated. Whether we want it to be based off the starting node (A*)
        /// or the ending node (Djikstra), or something else entirely. </summary>
        private float GetAppendedGCost(Node node, Node comparingNode)
        {
            return (node.Position - comparingNode.Position).magnitude;
        }

        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        private const int MOVE_ZDIAGONAL_COST = 14;
        /// <summary> We can customise the heuristic here. </summary>
        private float GetHeuristic(Node node, Node comparingNode)
        {
            float dx = (node.Position.x - comparingNode.Position.x).Abs();
            float dy = (node.Position.y - comparingNode.Position.y).Abs();
            float dz = (node.Position.z - comparingNode.Position.z).Abs();
            float dmin = Mathf.Min(dx, Mathf.Min(dy, dz));
            float dmax = Mathf.Max(dx, Mathf.Max(dy, dz));
            float dmid = dx + dy + dz - dmin - dmax;
            return (MOVE_ZDIAGONAL_COST - MOVE_DIAGONAL_COST) * dmin + (MOVE_DIAGONAL_COST - MOVE_STRAIGHT_COST) * dmid * MOVE_STRAIGHT_COST * dmax;
        }

        private Stack<Node> ReconstructPath(Node node, Node start)
        {
            Stack<Node> path = new Stack<Node>();
            while (node != start && node != null)
            {
                node.IsPath = true;
                path.Push(node);
                node = node.Parent;
            }
            if (watch != null)
            {
                watch.Stop();
                $"Elapsed time for A Star: {watch.ElapsedMilliseconds}".Msg();
            }
            return path;
        }

        /// <summary> Reconstructs a complete path from end node back to starting node in the form of a stack. </summary>
        private async Task<Stack<Node>> ReconstructPathAsync(Node node, Node start)
        {
            return await Task.Run(() =>
            {
                Stack<Node> path = new Stack<Node>();
                while (node != start && node != null)
                {
                    node.IsPath = true;
                    path.Push(node);
                    node = node.Parent;
                }
                if (watch != null)
                {
                    watch.Stop();
                    $"Elapsed time for A Star: {watch.ElapsedMilliseconds}".Msg();
                }
                return path;
            });
        }

        /// <summary> At the moment, neighbours are defined by their index positions in the 3D grid. We may have to
        /// change that if we are deleting nodes that are empty space. </summary>
        private List<Node> GetNeighbours(Node n)
        {
            //! Tweak this to change detection radius
            int MaxBound = maxBound > 0 ? maxBound : 1;
            int MinBound = -MaxBound;
            List<Node> list = new List<Node>();

            for (int x = MinBound; x <= MaxBound; x++)
            {
                for (int y = MinBound; y <= MaxBound; y++)
                {
                    for (int z = MinBound; z <= MaxBound; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        if (pos == Vector3Int.zero) continue;
                        pos += n.Index;
                        if (IsWithinBounds(pos.x, 0) && IsWithinBounds(pos.y, 1) && IsWithinBounds(pos.z, 2))
                        {
                            var node = grid.GetNodeAt(pos);
                            if (node != null && !node.HasObstacle)
                                list.Add(node);
                        }
                    }
                }
            }

            return list;

            bool IsWithinBounds(int a, int dimen) => a >= 0 && a < grid.Get.GetLength(dimen);
        }

        private Node GetNodePosition(Vector3 position, string nName = "")
        {
            Node foundNode = null;
            bool shouldBreak = false;
            grid.LoopNode_Breakable(node =>
            {
                if (node.Bounds.Contains(position) && !node.HasObstacle)
                {
                    $"Found position {nName}: {position}".Msg();
                    foundNode = node;
                    shouldBreak = true;
                    return;
                }
            }, () => shouldBreak);
            return foundNode;
        }
    }
}