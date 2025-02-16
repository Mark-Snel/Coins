#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonColliderVisualizer : MonoBehaviour
{
    public bool forceConvex = false; // Toggle convex/concave handling

    void Start()
    {
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Mesh generatedMesh = CreateMesh(polyCollider, forceConvex);
        meshFilter.mesh = generatedMesh;

        #if UNITY_EDITOR
        SaveMesh(generatedMesh, "PolygonColliderMesh");
        #endif
    }

    Mesh CreateMesh(PolygonCollider2D collider, bool forceConvex)
    {
        Vector2[] points = collider.points;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Convert 2D points to 3D
        foreach (Vector2 point in points)
        {
            vertices.Add(new Vector3(point.x, point.y, 0));
        }

        if (forceConvex)
        {
            // Simple fan triangulation (for convex shapes)
            for (int i = 1; i < points.Length - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }
        else
        {
            // Ear clipping triangulation for concave polygons
            TriangulateConcave(vertices, triangles);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void TriangulateConcave(List<Vector3> vertices, List<int> triangles)
    {
        List<int> remainingVertices = new List<int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            remainingVertices.Add(i);
        }

        while (remainingVertices.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < remainingVertices.Count; i++)
            {
                int prev = remainingVertices[(i - 1 + remainingVertices.Count) % remainingVertices.Count];
                int curr = remainingVertices[i];
                int next = remainingVertices[(i + 1) % remainingVertices.Count];

                if (IsEar(vertices, prev, curr, next, remainingVertices))
                {
                    triangles.Add(prev);
                    triangles.Add(curr);
                    triangles.Add(next);
                    remainingVertices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound) break;
        }

        if (remainingVertices.Count == 3)
        {
            triangles.Add(remainingVertices[0]);
            triangles.Add(remainingVertices[1]);
            triangles.Add(remainingVertices[2]);
        }
    }

    bool IsEar(List<Vector3> vertices, int prev, int curr, int next, List<int> remainingVertices)
    {
        Vector3 a = vertices[prev];
        Vector3 b = vertices[curr];
        Vector3 c = vertices[next];

        if (Vector3.Cross(b - a, c - a).z <= 0) return false; // Not a convex corner

        foreach (int index in remainingVertices)
        {
            if (index != prev && index != curr && index != next)
            {
                if (PointInTriangle(vertices[index], a, b, c))
                    return false;
            }
        }

        return true;
    }

    bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        float sign1 = (p.x - a.x) * (b.y - a.y) - (b.x - a.x) * (p.y - a.y);
        float sign2 = (p.x - b.x) * (c.y - b.y) - (c.x - b.x) * (p.y - b.y);
        float sign3 = (p.x - c.x) * (a.y - c.y) - (a.x - c.x) * (p.y - c.y);

        bool hasNeg = (sign1 < 0) || (sign2 < 0) || (sign3 < 0);
        bool hasPos = (sign1 > 0) || (sign2 > 0) || (sign3 > 0);

        return !(hasNeg && hasPos);
    }

    void SaveMesh(Mesh mesh, string meshName)
    {
        string path = "Assets/SavedMeshes/" + meshName + ".asset";

        if (!Directory.Exists("Assets/SavedMeshes"))
        {
            Directory.CreateDirectory("Assets/SavedMeshes");
        }

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh saved to " + path);
    }
}
#endif
