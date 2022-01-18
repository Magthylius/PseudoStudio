using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.UnitySoku;
using NaughtyAttributes;

namespace Hadal.AI
{
    public class DebugMesh : MonoBehaviour
    {
        [SerializeField] GameObject debugPrefab;
        List<Node> points = new List<Node>();

        [Button(nameof(DebugInfoMesh))]
        private void DebugInfoMesh()
        {
            points ??= new List<Node>();
            points.Clear();

            string name = gameObject.name;
            var mesh = GetComponent<MeshFilter>();
            List<Vector3> vertices = new List<Vector3>();
            mesh.mesh.GetVertices(vertices);

            foreach(var v in vertices)
            {
                var position = (transform.rotation * Vector3.Scale(v, transform.localScale)) + transform.position;
                Node node = new Node
                {
                    HasObstacle = true,
                    Position = position
                };
                points.Add(node);
                Instantiate(debugPrefab, position, Quaternion.identity, transform);
            }
            // foreach(var n in normals)
            // {
            //     var position = (transform.rotation * Vector3.Scale(n, transform.lossyScale / 2f)) + transform.position;
            //     Debug.DrawLine(position, position + (position - transform.position).normalized, Color.red, Mathf.Infinity);
            // }

            $"{name} Vertex count: {vertices.Count}".Msg();
            // $"{name} Normals count: {normals.Count}".Msg();
        }
    }
}
