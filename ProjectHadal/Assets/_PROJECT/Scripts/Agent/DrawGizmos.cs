using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class DrawGizmos : MonoBehaviour
    {
        Grid3D grid;
        bool isInitialised = false;
        BoxCollider collider;

        private void Update()
        {
            if (!isInitialised)
            {
                isInitialised = true;
                int dimensionSize = 5;
                int cellSize = 10;
                Vector3 centre = new Vector3(dimensionSize, dimensionSize, dimensionSize) * cellSize * 0.5f;
                Vector3 size = new Vector3(dimensionSize, dimensionSize, dimensionSize) * cellSize;
                Bounds bounds = new Bounds(centre, size);
                collider = GetComponent<BoxCollider>();
                collider.bounds.Encapsulate(bounds);
                grid = new Grid3D(dimensionSize, dimensionSize, dimensionSize, cellSize, collider);
            }
            
            grid.DoUpdate();
        }

        [ContextMenu(nameof(ResetInitialise))]
        private void ResetInitialise()
        {
            isInitialised = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(grid.GetCentre, grid.GetSize);
        }
    }

}
