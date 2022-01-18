using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class MergeMeshTest : MonoBehaviour
    {
        void Start()
        {
            MeshFilter self = GetComponent<MeshFilter>();
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                if (meshFilters[i] != self) Destroy(meshFilters[i].gameObject);
                i++;
            }
            transform.GetComponent<MeshFilter>().mesh = new Mesh();
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            transform.gameObject.SetActive(true);
        }

        // evil boiler plate
        void Update()
        {

        }
    }
}
