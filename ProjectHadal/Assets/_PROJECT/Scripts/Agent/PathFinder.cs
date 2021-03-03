using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class PathFinder : MonoBehaviour
    {
        Grid grid;

        public void SetGrid(Grid grid) => this.grid = grid;

        public async Task<Stack<Node>> FindAsync(Node from, Node to)
        {
            //! Setup for A*
            Node theStart = from;
            Node theEnd = to;
            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();
            List<Node> neighbours;
            Node current = theStart;
            open.Add(theStart);

            //! Setup node costs
            grid.LoopNode(node => node.ResetCosts());
            theStart.GCost = 0;
            theStart.FCost = GetHeuristic(theStart);

            //! Run A*
            while (open.IsNotEmpty())
            {
                current = open.First();
                if (current == theEnd)
                    return await ReconstructPath(current, theStart);
                
                open.Remove(current);
                closed.Add(current);
                neighbours = GetNeighbours(current);

                await Task.Run(() =>
                {
                    foreach (Node n in neighbours)
                    {
                        if (NodeIsNeverEvaluated(n) && !n.HasObstacle)
                        {
                            n.Parent = current;
                            n.GCost = current.GCost + GetAppendedGCost(n, theStart);
                            n.FCost = n.GCost + GetHeuristic(n);
                            open.Add(n);
                            open = open.OrderBy(node => node.FCost).ToList();
                        }
                    }
                }).ConfigureAwait(false);
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
            return Vector3.Distance(node.Position, comparingNode.Position);
        }

        /// <summary> We can customise the heuristic here. </summary>
        private float GetHeuristic(Node node)
        {
            return 1f; // sigmoid? ReLU?
        }

        /// <summary> Reconstructs a complete path from end node back to starting node in the form of a stack. </summary>
        private async Task<Stack<Node>> ReconstructPath(Node node, Node start)
        {
            return await Task.Run(() =>
            {
                Stack<Node> path = new Stack<Node>();
                while (node != start)
                {
                    path.Push(node);
                    node = node.Parent;
                }
                return path;
            });
        }

        /// <summary> At the moment, neighbours are defined by their index positions in the 3D grid. We may have to
        /// change that if we are deleting nodes that are empty space. </summary>
        private List<Node> GetNeighbours(Node n)
        {
            const int MinBound = -1;
            const int MaxBound = 1;
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
                            list.Add(grid.GetNodeAt(pos));
                        }
                    }
                }
            }

            return list;

            bool IsWithinBounds(int a, int dimen) => a >= 0 && a < grid.Get.GetLength(dimen);
        }
    }
}
