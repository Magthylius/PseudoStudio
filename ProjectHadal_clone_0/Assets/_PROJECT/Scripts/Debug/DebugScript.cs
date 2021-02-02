using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Hadal.Security;
using System.Linq;

//Created by Jet
namespace Hadal.Debugging
{
    using PlayerController = Controls.PlayerController;

    public class DebugScript : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private Vector3 direction;
        [SerializeField] private float speed;

        private void Update()
        {
            if (controller == null) return;
            if (Input.GetKey(KeyCode.V))
            {
                controller.AddVelocity(speed, direction);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                controller.transform.position = new Vector3(70, 20, 0);
            }
        }

        #region Test Bool Bit Array

        [ContextMenu(nameof(TestBoolBitArray))]
        private void TestBoolBitArray()
        {
            BoolBitArray bools = new BoolBitArray();
            bools[0] = true;
            bools[1] = false;
            bools[2] = true;
            bools[3] = true;
            bools[3] = false;

            for(int i = 0; i < 5; i++)
            {
                print($"Index: {i}, {bools[i]}.\n");
            }
        }

        #endregion

        #region Test Find 1

        private void TestFindObjectsWithLoop()
        {
            List<GameObject> oList = new List<GameObject>();
            foreach(var surface in FindObjectsOfType<NavMeshSurface>())
            {
                if(surface.gameObject.layer == LayerMask.NameToLayer("AINavigationLayer"))
                {
                    oList.Add(surface.gameObject);
                }
            }
            GameObject[] array = oList.ToArray();
        }

        #endregion

        #region Test Find 2

        private void TestFindObjectsWithLinq()
        {
            GameObject[] array = FindObjectsOfType<NavMeshSurface>()
                .Where(i => IsNagivationLayer(i))
                .Select(i => i.gameObject).ToArray();
        }

        private static bool IsNagivationLayer(NavMeshSurface i)
        {
            return i.gameObject.layer == LayerMask.NameToLayer("AINavigationLayer");
        }

        #endregion
    }
}