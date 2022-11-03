using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WingedEdge;
using HalfEdge;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorQuad : MonoBehaviour
{
    [SerializeField] bool m_DisplayEdges = true;
    [SerializeField] bool m_DisplayVertices = true;
    [SerializeField] bool m_DisplaySegment = true;

    [SerializeField] AnimationCurve m_Profile;
    delegate Vector3 ComputePosDelegate(float kX, float kZ);
    MeshFilter m_Mf;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        m_Mf.mesh = CreateStrip(1, new Vector3(.5f, .5f, .5f));
        //m_Mf.mesh = this.CreateGridXZ(3, 3, new Vector3(.5f, .5f, .5f));
        //m_Mf.mesh = this.CreateNormalizedG (ridXZ(6, 6); -
        /*m_Mf.mesh = this.CreateNormalizedGridXZ(30, 5, (kX, kZ) =>
        {
            float rho, theta, phi;

            // Coordinates mappings de (kX, kZ) vers (rho, theta, phi)
            theta = (1 - kX) * 2 * Mathf.PI;
            phi = kZ * Mathf.PI;
            rho = 2 + .55f * Mathf.Cos(kX * 2 * 8 * Mathf.PI) * Mathf.Sin(kZ * 2 * 6 * Mathf.PI);
            //rho = 3 + .25f * Mathf.Sin(kZ * 2 * Mathf.PI * 4);
            //rho = m_Profile.Evaluate(kZ) * 2;
            return new Vector3(
                rho * Mathf.Cos(theta) * Mathf.Sin(phi),
                rho * Mathf.Cos(phi),
                rho * Mathf.Sin(theta) * Mathf.Sin(phi));
            //return new Vector3(Mathf.Lerp(-5, 5, kX), 0, Mathf.Lerp(-3, 3, kZ));
        });*/

        //Create a donut mesh with a hole in the middle and using CreateNormalizedGridXZ
        /*m_Mf.mesh = this.CreateNormalizedGridXZ(6 * 100, 50, (kX, kZ) =>
        {
            float R = 3;
            float r = 1;
            float theta = 6 * 2 * Mathf.PI * kX;
            Vector3 OOmega = new Vector3(R * Mathf.Cos(theta), 0, R * Mathf.Sin(theta));
            float alpha = Mathf.PI * kZ * 2 ;
            Vector3 OOmegaP = r * Mathf.Cos(alpha) * OOmega.normalized + r * Mathf.Sin(alpha) * Vector3.up
            + Vector3.up * kX * 2 * r * 6;
            return OOmega + OOmegaP;
        });*/

        //m_Mf.mesh = CreateBox(new Vector3(5, 5, 5));
        //m_Mf.mesh = CreateChips(new Vector3(5, 5, 5));
        //m_Mf.mesh = this.CreateRegularPolygon(new Vector3(8, 0, 8), 20);
        //m_Mf.mesh = this.CreatePacman(new Vector3(8, 0, 8), 20);

        //WingedEdgeMesh wingedEdgeMesh = new WingedEdgeMesh(m_Mf.mesh);
        //GUIUtility.systemCopyBuffer = wingedEdgeMesh.ConvertToCSVFormat("\t");
        //m_Mf.mesh = wingedEdgeMesh.ConvertToFaceVertexMesh();

        HalfEdgeMesh halfEdgeMesh = new HalfEdgeMesh(m_Mf.mesh);
        halfEdgeMesh.SubdivideCatmullClark();
        GUIUtility.systemCopyBuffer = halfEdgeMesh.ConvertToCSVFormat("\t");
        m_Mf.mesh = halfEdgeMesh.ConvertToFaceVertexMesh();

        //GUIUtility.systemCopyBuffer = ConvertToCSV("\t");
        //Debug.Log(ConvertToCSV("\t"));
    }

    string ConvertToCSV(string separator)
    {
        if (!(m_Mf.mesh && m_Mf.mesh))
        {
            return "";
        }

        Vector3[] vertices = m_Mf.mesh.vertices;
        int[] quads = m_Mf.mesh.GetIndices(0);
        List<string> strings = new List<string>();

        for (int i = 0; i < m_Mf.mesh.vertices.Length; i++)
        {
            Vector3 pos = vertices[i];

            strings.Add(i.ToString() + separator +
                pos.x.ToString("N02") + " " +
                pos.y.ToString("N02") + " " +
                pos.z.ToString("N02") + separator + separator
            );
        }

        for (int i = vertices.Length; i < quads.Length / 4; i++)
        {
            strings.Add(separator + separator + separator);
        }

        for (int i = 0; i < quads.Length / 4; i++)
        {
            strings[i] += i.ToString() + separator +
            quads[i * 4 + 0].ToString() + ", " +
            quads[i * 4 + 1].ToString() + ", " +
            quads[i * 4 + 2].ToString() + ", " +
            quads[i * 4 + 3].ToString();
        }

        return "Vertices" + separator + separator + separator + "Faces\n" +
        "Index" + separator + "Position" + separator + separator + "Index" + separator + "Indices des vertices" + "\n" +
        string.Join("\n", strings);
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
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {
            float kz = (float)i / nSegmentsZ;  // coefficient d'avancement sur la boucle, entre 0 et 100%
            for (int j = 0; j < nSegmentsX + 1; j++)
            {
                float kx = (float)j / nSegmentsX;  // coefficient d'avancement sur la boucle, entre 0 et 100%
                vertices[index++] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, kx), 0, Mathf.Lerp(-halfSize.z, halfSize.z, kz));
            }
        }

        index = 0;
        for (int i = 0; i < nSegmentsZ; i++)
        {
            for (int j = 0; j < nSegmentsX; j++)
            {
                quads[index++] = i * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j + 1;
                quads[index++] = i * (nSegmentsX + 1) + j + 1;

            }
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0); ;

        return mesh;
    }

    Mesh CreateNormalizedGridXZ(int nSegmentsX, int nSegmentsZ, ComputePosDelegate computePos = null)
    {
        Mesh mesh = new Mesh();
        mesh.name = "normalizedgrid";

        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsX * nSegmentsZ * 4];

        int index = 0;
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {
            float kz = (float)i / nSegmentsZ;  // coefficient d'avancement sur la boucle, entre 0 et 100%
            for (int j = 0; j < nSegmentsX + 1; j++)
            {
                float kx = (float)j / nSegmentsX;  // coefficient d'avancement sur la boucle, entre 0 et 100%
                vertices[index++] = computePos != null ? computePos(kx, kz) : new Vector3(kx, 0, kz);
            }
        }

        index = 0;
        for (int i = 0; i < nSegmentsZ; i++)
        {
            for (int j = 0; j < nSegmentsX; j++)
            {
                quads[index++] = i * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j + 1;
                quads[index++] = i * (nSegmentsX + 1) + j + 1;

            }
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0); ;

        return mesh;
    }

    Mesh CreateBox(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Box";

        Vector3[] vertices = new Vector3[8]; // 8 points (=vertice) pour créer une box
        int[] quads = new int[4 * 6];

        vertices[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        vertices[1] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        vertices[2] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        vertices[3] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);

        vertices[4] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        vertices[5] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
        vertices[6] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        vertices[7] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);

        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        quads[4] = 3;
        quads[5] = 2;
        quads[6] = 5;
        quads[7] = 4;

        quads[8] = 4;
        quads[9] = 5;
        quads[10] = 6;
        quads[11] = 7;

        quads[12] = 5;
        quads[13] = 2;
        quads[14] = 1;
        quads[15] = 6;

        quads[16] = 7;
        quads[17] = 6;
        quads[18] = 1;
        quads[19] = 0;

        quads[20] = 4;
        quads[21] = 7;
        quads[22] = 0;
        quads[23] = 3;

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreateChips(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "chips";

        Vector3[] vertices = new Vector3[8];
        int[] quads = new int[3 * 4];

        // Face 0 (0, 1, 2, 3)
        vertices[0] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        vertices[1] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        vertices[2] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        vertices[3] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);

        // Face 1 (4, 5, 6, 7)
        vertices[4] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
        vertices[5] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        vertices[6] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        vertices[7] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);

        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        quads[4] = 4;
        quads[5] = 5;
        quads[6] = 6;
        quads[7] = 7;

        quads[8] = 4;
        quads[9] = 1;
        quads[10] = 0;
        quads[11] = 5;

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreateRegularPolygon(Vector3 halfSize, int nSectors)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Polygon";

        Vector3[] vertices = new Vector3[(nSectors * 2) + 1];
        int[] quads = new int[(4 * nSectors)];


        float initialAngle = (360 / nSectors) * Mathf.PI / 180; // en radian
        float currentAngle = initialAngle;

        for (int i = 0; i < (nSectors * 2) + 1; i += 2)
        {
            vertices[i] = new Vector3(
                Mathf.Cos(currentAngle) * halfSize.x,
                0,
                Mathf.Sin(currentAngle) * halfSize.x);

            currentAngle += initialAngle;
        }

        for (int i = 1; i < (nSectors * 2); i += 2)
        {
            float milieu_x = (vertices[i - 1].x + vertices[i + 1].x) / 2;
            float milieu_z = (vertices[i - 1].z + vertices[i + 1].z) / 2;
            vertices[i] = new Vector3(milieu_x, 0, milieu_z);
        }

        vertices[nSectors * 2] = Vector3.zero;

        int index = 0;
        int lastVertice = vertices.Length - 1;
        int beforeLastVertice = lastVertice - 1;
        for (int i = 0; i < quads.Length / 2; i += 2)
        {
            beforeLastVertice = i == 0 ? beforeLastVertice : i - 1;
            quads[index++] = lastVertice;
            quads[index++] = i + 1;
            quads[index++] = i;
            quads[index++] = beforeLastVertice;

        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreatePacman(Vector3 halfSize, int nSectors, float startAngle = Mathf.PI / 3, float endAngle = 5 * Mathf.PI / 3)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Pacman";

        Vector3[] vertices = new Vector3[(nSectors * 2) + 2];
        int[] quads = new int[(4 * nSectors)];


        float initialAngle = ((endAngle - startAngle) / nSectors); // en radian
        float currentAngle = initialAngle;

        for (int i = 0; i < (nSectors * 2) + 1; i += 2)
        {
            vertices[i] = new Vector3(
                Mathf.Cos(currentAngle) * halfSize.x,
                0,
                Mathf.Sin(currentAngle) * halfSize.x);

            currentAngle += initialAngle;
        }

        for (int i = 1; i < (nSectors * 2); i += 2)
        {
            float milieu_x = (vertices[i - 1].x + vertices[i + 1].x) / 2;
            float milieu_z = (vertices[i - 1].z + vertices[i + 1].z) / 2;
            vertices[i] = new Vector3(milieu_x, 0, milieu_z);
        }

        vertices[(nSectors * 2) + 1] = Vector3.zero;

        int index = 0;
        int lastVertice = vertices.Length - 1;
        int beforeLastVertice = lastVertice - 1;
        for (int i = 1; i < quads.Length / 2; i += 2)
        {
            beforeLastVertice = i == 0 ? 0 : i - 1;
            quads[index++] = lastVertice;
            quads[index++] = i + 1;
            quads[index++] = i;
            quads[index++] = beforeLastVertice;

        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    private void OnDrawGizmos()
    {
        if (!(m_Mf && m_Mf.mesh) || !this.m_DisplayEdges)
            return;

        Mesh mesh = m_Mf.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);


        GUIStyle style = new GUIStyle();
        if (this.m_DisplayVertices)
        {
            style.fontSize = 20;
            style.normal.textColor = Color.red;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                Handles.Label(worldPos, i.ToString() + " " + worldPos.ToString(), style);
            }
        }

        if (this.m_DisplaySegment)
        {
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

                if (this.m_DisplayVertices)
                {
                    string str = string.Format("{0}:{1},{2},{3},{4}", i, index1, index2, index3, index4);
                    Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
                }
            }
        }
    }
}