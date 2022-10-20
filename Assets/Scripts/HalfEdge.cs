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

    Vector3[] vertices = new Vector3[this.vertices.Count];
    int[] quads = new int[faces.Count * 4];

    foreach (Vertex v in this.vertices)
    {
        vertices[v.index] = v.position;
    }

    for (int i = 0; i < faces.Count; i++)
    {
        Face f = faces[i];
        int offset = 0;
        HalfEdge he = f.edge.prevEdge;
        do
        {
            quads[i * 4 + offset] = he.sourceVertex.index;
            offset++;
            he = he.nextEdge;
        }
        while (f.edge.prevEdge != he);
    }		
    newMesh.vertices = vertices;
    newMesh.SetIndices(quads, MeshTopology.Quads, 0);
    newMesh.RecalculateBounds();
    newMesh.RecalculateNormals();
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