#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonColliderVisualizer : MonoBehaviour
{
    void Start()
    {
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        // Create a basic material that works for sprites (you can change this as needed)
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Generate the mesh based on the collider's paths.
        Mesh generatedMesh = GenerateMeshFromPolygonCollider(polyCollider);
        meshFilter.mesh = generatedMesh;

        // Save the mesh as an asset in the Editor.
        #if UNITY_EDITOR
        SaveMesh(generatedMesh, "PolygonColliderMesh");
        #endif
    }

    Mesh GenerateMeshFromPolygonCollider(PolygonCollider2D polyCollider)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int vertexOffset = 0;

        // Process each path in the collider.
        for (int i = 0; i < polyCollider.pathCount; i++)
        {
            Vector2[] path = polyCollider.GetPath(i);

            // Create a Triangulator instance to process the current path.
            Triangulator triangulator = new Triangulator(path);
            int[] indices = triangulator.Triangulate();

            // Add vertices and a simple UV (using the same coordinates; adjust if needed)
            foreach (Vector2 point in path)
            {
                vertices.Add(new Vector3(point.x, point.y, 0));
                uvs.Add(point);
            }

            // Offset the triangle indices by the current vertex count.
            foreach (int index in indices)
            {
                triangles.Add(vertexOffset + index);
            }
            vertexOffset += path.Length;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PolygonColliderMesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
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

// Helper class for triangulating a polygon (ear clipping method)
public class Triangulator
{
    private List<Vector2> m_points;

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a = V[u], b = V[v], c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A * 0.5f;
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;

        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax = C.x - B.x, ay = C.y - B.y;
        float bx = A.x - C.x, by = A.y - C.y;
        float cx = B.x - A.x, cy = B.y - A.y;
        float apx = P.x - A.x, apy = P.y - A.y;
        float bpx = P.x - B.x, bpy = P.y - B.y;
        float cpx = P.x - C.x, cpy = P.y - C.y;

        float aCROSSbp = ax * bpy - ay * bpx;
        float cCROSSap = cx * apy - cy * apx;
        float bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
#endif
