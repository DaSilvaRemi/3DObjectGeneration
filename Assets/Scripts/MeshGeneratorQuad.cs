using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorQuad : MonoBehaviour
{
    MeshFilter m_Mf;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = this.CreateGridXZ(16, 16, new Vector3(8, 0, 8));
    }

    Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[(nSegments + 1) * 2];
        int[] quads = new int[nSegments * 4];

        int index = 0;
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);
        for (int i = 0; i <= nSegments; i++)
        {
            float k = (float)i / nSegments; // coefficient d'avancement sur la boucle, entre 0 et 100%
            Vector3 tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos; // vertice du haut-gauche
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward; // vertice du bas-droite
        }

        index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            quads[index++] = 2 * i; // Vertices créés : |/|/|/|/
            quads[index++] = 2 * i + 2;
            quads[index++] = 2 * i + 3;
            quads[index++] = 2 * i + 1;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "grid";

        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsX * nSegmentsZ * 4];

        int index = 0;
        for (int i = 0; i < nSegmentsX + 1; i++)
        {
            for (int j = 0; j < nSegmentsZ + 1; j++)
            {
                float k = (float) i / nSegmentsX;  // coefficient d'avancement sur la boucle, entre 0 et 100%
                float l = (float) j / nSegmentsZ;  // coefficient d'avancement sur la boucle, entre 0 et 100%
                Vector3 tmpPos = new Vector3(-halfSize.x + 2 * halfSize.x * k, 0, -halfSize.z + 2 * halfSize.z * l);
                vertices[index++] = tmpPos;
            }
        }

        index = 0;
        for (int i = 0; i < nSegmentsX; i++)
        {
            for (int j = 0; j < nSegmentsZ; j++)
            {
                quads[index++] = i + j * (nSegmentsX + 1); // Vertices créés : |/|/|/|/
                quads[index++] = i + 1 + j * (nSegmentsX + 1);
                quads[index++] = i + 1 + (j + 1) * (nSegmentsX + 1);
                quads[index++] = i + (j + 1) * (nSegmentsX + 1);
                
            }
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);;

        return mesh;
    }

    private void OnDrawGizmos()
    {
        if (!(m_Mf && m_Mf.mesh))
            return;

        Mesh mesh = m_Mf.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);


        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.red;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            Handles.Label(worldPos, i.ToString(), style);
        }

        Gizmos.color = Color.black;
        style.fontSize = 15;
        style.normal.textColor = Color.blue;

        for (int i = 0; i < quads.Length / 4; i++)
        {
            int index1 = quads[4 * i];
            int index2 = quads[4 * i + 1];
            int index3 = quads[4 * i + 2];
            int index4 = quads[4 * i + 3];


            Vector3 pt1 = transform.TransformPoint(vertices[index1]);
            Vector3 pt2 = transform.TransformPoint(vertices[index2]);
            Vector3 pt3 = transform.TransformPoint(vertices[index3]);
            Vector3 pt4 = transform.TransformPoint(vertices[index4]);

            Gizmos.DrawLine(pt1, pt2);
            Gizmos.DrawLine(pt2, pt3);
            Gizmos.DrawLine(pt3, pt4);
            Gizmos.DrawLine(pt4, pt1);

            string str = string.Format("{0}:{1},{2},{3},{4}", i, index1, index2, index3, index4);
            Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
        }
    }
}