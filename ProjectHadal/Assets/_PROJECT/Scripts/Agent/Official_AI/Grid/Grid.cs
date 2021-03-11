using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Hadal.AI
{
    public class Grid
    {
        public Grid(Node[,,] grid) => this.grid = grid;
        public Node[,,] grid;
        public Node[,,] Get => grid;
        public Node GetNodeAt(Vector3Int pos) => Get[pos.x, pos.y, pos.z];
        public Node[] GetAs1DArray()
        {
            Node[] nodes = new Node[Get.Length];
            int i = 0;
            for (int x = 0; x < Get.GetLength(0); x++)
            {
                for (int y = 0; y < Get.GetLength(1); y++)
                {
                    for (int z = 0; z < Get.GetLength(2); z++)
                    {
                        nodes[i++] = Get[x, y, z];
                    }
                }
            }
            return nodes;
        }

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
        public async Task LoopAs1DArray_XNodesPerIterationAsync(Action<Node[]> method, CancellationToken tolkien, int steps)
        {
            Node[] nodes = GetAs1DArray();

            int step = steps;
            for (int i = 0; i < nodes.Length; i += step)
            {
                Node[] abcdefghij = new Node[step];
                for (int s = 0; s < step; s++)
                    abcdefghij[s] = GetPositionIfAny(nodes, i + s);

                await Task.Run(() => method.Invoke(abcdefghij), tolkien);
            }
        }

        Node GetPositionIfAny(Node[] nodes, int index)
        {
            if (index >= 0 && index < nodes.Length)
                return nodes[index];

            return null;
        }
    }
}