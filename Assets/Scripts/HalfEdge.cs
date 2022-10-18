using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace HalfEdge
{
public class HalfEdge
{
public int index;
public Vertex sourceVertex;
public Face face;
public HalfEdge prevEdge;
public HalfEdge nextEdge;
public HalfEdge twinEdge;
}
public class Vertex
{
public int index;
public Vector3 position;
public HalfEdge outgoingEdge;
}
public class Face
{
public int index;
public HalfEdge edge;
}
public class HalfEdgeMesh
{
public List<Vertex> vertices;
public List<HalfEdge> edges;
public List<Face> faces;
public HalfEdgeMesh(Mesh mesh)
{ // constructeur prenant un mesh Vertex-Face en paramètre
//magic happens

}
public Mesh ConvertToFaceVertexMesh()
{
    Mesh newMesh = new Mesh();

    /*Vector3[] vertices = new Vector3[vertices.Count];
    int[] quads = new int[faces.Count * 4];

    foreach (Vertex v in vertices)
    {
        vertices[v.index] = v.m_position;
    }

    int idQuad = 0;
    foreach (Face f in m_Faces)
    {
        int offset = 0;
        HalfEdge he = f.m_side.m_prevEdge;
        do
        {
            quads[idQuad * 4 + offset] = he.m_sourceVertex.m_index;
            offset++;
            he = he.m_nextEdge;
        }
        while (f.m_side.m_prevEdge != he);
        idQuad++;
    }		
    newMesh.vertices = vertices;
    newMesh.SetIndices(quads, MeshTopology.Quads, 0);
    newMesh.RecalculateBounds();
    newMesh.RecalculateNormals();*/
    return newMesh;
}
public string ConvertToCSVFormat(string separator = "\t")
{
string str = "";
//magic happens
return str;
}
public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces)
{
//magic happens
}
}
}