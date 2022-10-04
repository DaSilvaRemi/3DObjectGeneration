using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorTriangles : MonoBehaviour
{
    MeshFilter m_Mf;

    private void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        m_Mf.mesh = CreateTriangle();
        m_Mf.mesh = CreateQuad(new Vector3(1, 0, 2));
        m_Mf.mesh = CreateStrip(16, new Vector3(1, 0, 8));
    }

    Mesh CreateTriangle()
    {
        Mesh mesh = new Mesh();
        mesh.name = "triangle";

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[1 * 3];

        vertices[0] = Vector3.right;
        vertices[1] = Vector3.up;
        vertices[2] = Vector3.forward;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateQuad(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "quad";

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2 * 3];

        vertices[0] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[1] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[4] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[2 * (nSegments+1)];
        int[] triangles = new int[nSegments * 2 * 3];

        int index = 0;
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);
        for (int i = 0; i < nSegments + 1 ; i++)
        {
            float k = i / nSegments;
            Vector3 tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos;
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward;
        }

        index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            triangles[index++] = 2 * i;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 1;

            triangles[index++] = 2 * i + 1;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateGridXZ(int segmentsX, int segmentsZ, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[2 * (1 + 1)];
        int[] triangles = new int[1 * 2 * 3];

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }
}
