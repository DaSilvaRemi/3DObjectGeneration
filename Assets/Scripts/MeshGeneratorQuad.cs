using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WingedEdge;
using HalfEdge;
using Unity.Mathematics;
using static Unity.Mathematics.math;

delegate Vector3 ComputePosDelegate(float kX, float kZ);
delegate float3 ComputePosDelegate_SIMD(float3 k);

enum Shapes
{
    STRIP,
    GRID,
    NormalizeGRID,
    BOX,
    CHIPS,
    REGULAR_POLYGON,
    PACMAN,
    DIAMOND,
    DIAMOND_WITH_HOLES,
}

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorQuad : MonoBehaviour
{
    [SerializeField] bool m_DisplayMeshInfo = true;
    [SerializeField] bool m_DisplayMeshEdges = true;
    [SerializeField] bool m_DisplayMeshVertices = true;
    [SerializeField] bool m_DisplayMeshFaces = true;
    [SerializeField] bool m_DoCatmullClarck = false;
    [SerializeField] int m_NbSubdivision = 0;
    [SerializeField] Shapes m_SelectedShapes = Shapes.BOX;

    [SerializeField] AnimationCurve m_Profile;

    MeshFilter m_Mf;
    WingedEdgeMesh m_WingedEdgeMesh;
    HalfEdgeMesh m_HalfEdgeMesh;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
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

        //bool bothSides = true;
        //m_Mf.mesh = CreateNormalizedGridXZ_SIMD((bothSides ? 2 : 1) * int3(200, 200, 1),
        //    (k) => {
        //        if (bothSides) k = abs((k - .5f) * 2);
        //        //return lerp(float3(-5f, 0, -5f), float3(5f, 0, 5f), k.xzy);
        //        //return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(k.x, step(.2f, k.x), k.y));
        //        //return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(k.x, smoothstep(0.5f-0.1f, 0.5f+0.1f, k.x), k.y));
        //        return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(
        //            k.x,
        //            0.5f - 0.1f + 0.5f + 0.1f + 0.5f * sin(k.x * 2 * PI * 4) * cos(k.y * 2 * PI * 3) + 1,
        //            //smoothstep(0.5f - 0.1f, 0.5f + 0.1f, 0.5f * sin(k.x * 2 * PI * 4) * cos(k.y * 2 * PI * 3) + 1),
        //            k.y
        //        ));
        //    }
        //);

        //int3 nCells = int3(3, 3, 1);
        //int3 nSegmentsPerCell = int3(100, 100, 1);
        //float3 kStep  = float3(1) / (nCells*nSegmentsPerCell);

        //float3 cellSize = float3(1, .5f, 1);

        // int3 nCells = int3(3, 3, 1);

        // int3 nSegmentsPerCell = int3(100, 100, 1);
        // float3 kStep = float3(1) / (nCells * nSegmentsPerCell);
        // float3 cellSize = float3(1, .5f, 1);

        // m_Mf.mesh = CreateNormalizedGridXZ_SIMD(

        //     nCells * nSegmentsPerCell,

        //     (k) =>

        //     {
        //         // calculs sur la grille normalisée
        //         int3 index = (int3)floor(k / kStep);
        //         int3 localIndex = index % nSegmentsPerCell;
        //         int3 indexCell = index / nSegmentsPerCell;
        //         float3 relIndexCell = (float3)indexCell / nCells;

        //         // calculs sur les positions dans l'espace
        //         /*
        //         float3 cellOriginPos = lerp(
        //             -cellSize * nCells.xzy * .5f,
        //             cellSize * nCells.xzy * .5f,
        //             relIndexCell.xzy);

        //         */
        //         float3 cellOriginPos = floor(k * nCells).xzy; // Theo's style ... ne prend pas en compte cellSize
        //         k = frac(k * nCells);
        //         return cellOriginPos + cellSize * float3(k.x, smoothstep(0.2f - .05f, .2f + .05f, k.x * k.y), k.y);
        //     }
        //     );

        switch (this.m_SelectedShapes)
        {
            case Shapes.STRIP:
                m_Mf.mesh = CreateStrip(1, new Vector3(.5f, .5f, .5f));
                break;
            case Shapes.GRID:
                m_Mf.mesh = this.CreateGridXZ(20, 20, new Vector3(.5f, .5f, .5f));
                break;
            case Shapes.NormalizeGRID:
                m_Mf.mesh = this.CreateNormalizedGridXZ(6, 6);
                break;
            case Shapes.BOX:
                m_Mf.mesh = CreateBox(new Vector3(1, 1, 1));
                break;
            case Shapes.CHIPS:
                m_Mf.mesh = CreateChips(new Vector3(1, 1, 1));
                break;
            case Shapes.REGULAR_POLYGON:
                m_Mf.mesh = this.CreateRegularPolygon(new Vector3(5, 0, 5), 20);
                break;
            case Shapes.PACMAN:
                m_Mf.mesh = this.CreatePacman(new Vector3(5, 0, 5), 20);
                break;
            case Shapes.DIAMOND:
                m_Mf.mesh = this.CreateDiamond(new Vector3(2, 2, 2));
                break;
            case Shapes.DIAMOND_WITH_HOLES:
                m_Mf.mesh = this.CreateDiamondWithHoles(new Vector3(2, 2, 2));
                break;
            default:
                break;
        }

        //this.m_WingedEdgeMesh = new WingedEdgeMesh(m_Mf.mesh);
        //GUIUtility.systemCopyBuffer = this.m_WingedEdgeMesh.ConvertToCSVFormat("\t");
        //this.m_Mf.mesh = this.m_WingedEdgeMesh.ConvertToFaceVertexMesh();
        //Debug.Log(this.m_WingedEdgeMesh.ConvertToCSVFormat("\t"));

        this.m_HalfEdgeMesh = new HalfEdgeMesh(m_Mf.mesh);

        if (this.m_DoCatmullClarck)
        {
            this.m_HalfEdgeMesh.SubdivideCatmullClark(this.m_NbSubdivision);
        }

        GUIUtility.systemCopyBuffer = this.m_HalfEdgeMesh.ConvertToCSVFormat("\t");
        this.m_Mf.mesh = this.m_HalfEdgeMesh.ConvertToFaceVertexMesh();
        Debug.Log(this.m_HalfEdgeMesh.ConvertToCSVFormat("\t"));

        //GUIUtility.systemCopyBuffer = this.ConvertToCSV("\t");
        //Debug.Log(this.ConvertToCSV("\t"));
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
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

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

    Mesh CreateNormalizedGridXZ_SIMD(int3 nSegments, ComputePosDelegate_SIMD computePos = null)
    {
        Mesh mesh = new Mesh();
        mesh.name = "normalizedgrid";

        Vector3[] vertices = new Vector3[(nSegments.x + 1) * (nSegments.y + 1)];
        int[] quads = new int[nSegments.x * nSegments.y * 4];

        int index = 0;
        for (int i = 0; i < nSegments.y + 1; i++)
        {
            for (int j = 0; j < nSegments.x + 1; j++)
            {
                float3 k = float3(j, i, 0) / nSegments;  // coefficient d'avancement sur la boucle, entre 0 et 100%
                vertices[index++] = computePos != null ? computePos(k) : k;
            }
        }

        index = 0;
        int offset = 0;
        int nextOffset = offset;
        for (int i = 0; i < nSegments.y; i++)
        {
            nextOffset = offset + nSegments.x + 1;
            for (int j = 0; j < nSegments.x; j++)
            {
                quads[index++] = offset + j;
                quads[index++] = nextOffset + j;
                quads[index++] = nextOffset + j + 1;
                quads[index++] = offset + j + 1;

            }
            offset = nextOffset;
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

    Mesh CreateDiamond(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "diamond";

        Vector3[] vertices = new Vector3[10];
        int[] quads = new int[8 * 4];

        // TOP
        vertices[0] = new Vector3(0, halfSize.y, 0);

        // Intermediate quad
        vertices[1] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, 0);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);
        vertices[4] = new Vector3(0, 0, -halfSize.z);
        vertices[5] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[6] = new Vector3(-halfSize.x, 0, 0);
        vertices[7] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[8] = new Vector3(0, 0, halfSize.z);

        // Bottom
        vertices[9] = new Vector3(0, -halfSize.y, 0);

        // UPPER 3D TRIANGLE
        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        quads[4] = 0;
        quads[5] = 3;
        quads[6] = 4;
        quads[7] = 5;

        quads[8] = 0;
        quads[9] = 5;
        quads[10] = 6;
        quads[11] = 7;

        quads[12] = 0;
        quads[13] = 7;
        quads[14] = 8;
        quads[15] = 1;

        // Bottom Triangle
        quads[16] = 9;
        quads[17] = 3;
        quads[18] = 2;
        quads[19] = 1;

        quads[20] = 9;
        quads[21] = 5;
        quads[22] = 4;
        quads[23] = 3;

        quads[24] = 9;
        quads[25] = 7;
        quads[26] = 6;
        quads[27] = 5;

        quads[28] = 9;
        quads[29] = 1;
        quads[30] = 8;
        quads[31] = 7;

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    Mesh CreateDiamondWithHoles(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "diamond_hole";

        Vector3[] vertices = new Vector3[10];
        int[] quads = new int[6 * 4];

        // TOP
        vertices[0] = new Vector3(0, halfSize.y, 0);

        // Intermediate quad
        vertices[1] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, 0);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);
        vertices[4] = new Vector3(0, 0, -halfSize.z);
        vertices[5] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[6] = new Vector3(-halfSize.x, 0, 0);
        vertices[7] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[8] = new Vector3(0, 0, halfSize.z);

        // Bottom
        vertices[9] = new Vector3(0, -halfSize.y, 0);

        // UPPER 3D TRIANGLE
        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        // quads[4] = 0;
        // quads[5] = 3;
        // quads[6] = 4;
        // quads[7] = 5;

        quads[4] = 0;
        quads[5] = 5;
        quads[6] = 6;
        quads[7] = 7;

        quads[8] = 0;
        quads[9] = 7;
        quads[10] = 8;
        quads[11] = 1;

        // Bottom Triangle
        quads[12] = 9;
        quads[13] = 3;
        quads[14] = 2;
        quads[15] = 1;

        quads[16] = 9;
        quads[17] = 5;
        quads[18] = 4;
        quads[19] = 3;

        quads[20] = 9;
        quads[21] = 7;
        quads[22] = 6;
        quads[23] = 5;

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
        if (!(m_Mf && m_Mf.mesh && m_DisplayMeshInfo)) return;

        if (this.m_WingedEdgeMesh != null)
        {
            this.m_WingedEdgeMesh.DrawGizmos(this.m_DisplayMeshVertices, this.m_DisplayMeshEdges, this.m_DisplayMeshFaces, transform);
        }

        if (this.m_HalfEdgeMesh != null)
        {
            this.m_HalfEdgeMesh.DrawGizmos(this.m_DisplayMeshVertices, this.m_DisplayMeshEdges, this.m_DisplayMeshFaces, transform);
        }

        Mesh mesh = m_Mf.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);


        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.normal.textColor = Color.red;


        if (m_DisplayMeshVertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                Handles.Label(worldPos, i.ToString(), style);
            }
        }

        Gizmos.color = Color.black;
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


            if (m_DisplayMeshEdges)
            {
                Gizmos.DrawLine(pt1, pt2);
                Gizmos.DrawLine(pt2, pt3);
                Gizmos.DrawLine(pt3, pt4);
                Gizmos.DrawLine(pt4, pt1);
            }

            if (m_DisplayMeshFaces)
            {
                string str = string.Format("{0} ({1},{2},{3},{4})", i, index1, index2, index3, index4);
                Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
            }
        }

    }
}