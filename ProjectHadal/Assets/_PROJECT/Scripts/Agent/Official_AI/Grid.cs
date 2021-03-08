using System;
using UnityEngine;

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
}