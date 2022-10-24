using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace WingedEdge
{
    public class WingedEdge
    {
        public int index;
        public Vertex startVertex;

        public Vertex endVertex;
        public Face leftFace;
        public Face rightFace;
        public WingedEdge startCWEdge;
        public WingedEdge startCCWEdge;
        public WingedEdge endCWEdge;
        public WingedEdge endCCWEdge;

        public WingedEdge()
        {

        }

        public WingedEdge(int index, Vertex startVertex, Vertex endVertex, Face leftFace, Face rightFace)
        {
            this.index = index;
            this.startVertex = startVertex;
            this.endVertex = endVertex;
            this.leftFace = leftFace;
            this.rightFace = rightFace;
        }
    }
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public WingedEdge edge;

        public Vertex()
        {

        }

        public Vertex(int index, Vector3 pos)
        {
            this.index = index;
            this.position = pos;
        }
    }
    public class Face
    {
        public int index;
        public WingedEdge edge;

        public Face() : this(0)
        {

        }

        public Face(int index)
        {
            this.index = index;
        }
    }

    public class WingedEdgeMesh
    {
        public List<Vertex> vertices;
        public List<WingedEdge> edges;
        public List<Face> faces;

        public WingedEdgeMesh(Mesh mesh)
        {
            // constructeur prenant un mesh Vertex-Face en parametre
            this.vertices = new List<Vertex>();
            this.edges = new List<WingedEdge>();
            this.faces = new List<Face>();
            Vector3[] meshVertices = mesh.vertices;

            // For each vertex
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                this.vertices.Add(new Vertex(i, meshVertices[i]));
            }

            int[] shapes = mesh.GetIndices(0);

            MeshTopology meshTopology = mesh.GetTopology(0);

            int nVerticesForTopology = meshTopology.Equals(MeshTopology.Triangles) ? 3 : 4;
            int indexFace = 0;

            Dictionary<long, WingedEdge> mapWingedEdges = new Dictionary<long, WingedEdge>();

            int nbFaces = shapes.Length / nVerticesForTopology;
            // For each face
            for (int i = 0; i < nbFaces; i++)
            {
                // We create two new faces
                Face leftFace = new Face(indexFace++);
                Face rightFace = new Face(indexFace++);

                // Create vertex
                for (int j = 0; j < nVerticesForTopology - 1; j += 2)
                {
                    // Get start and end vertex
                    Vertex vStart = this.vertices[shapes[j]];
                    Vertex vEnd = this.vertices[shapes[j + 1]];
                    // Create winged edge
                    WingedEdge wingedEdge = new WingedEdge(j, vStart, vEnd, leftFace, rightFace);
                    leftFace.edge = wingedEdge;
                    rightFace.edge = wingedEdge;

                    long key = j + ((j + 1) << 32);
                    mapWingedEdges.Add(key, wingedEdge);
                }

                // Connect edges
                for (int j = 0; j < mapWingedEdges.Count; j++)
                {
                    KeyValuePair<long, WingedEdge> kp = mapWingedEdges.ElementAt(j);
                    long key = kp.Key;
                    WingedEdge wingedEdge = kp.Value;

                    // Get start and end edges
                    long keyStartCW = key + 1;
                    long keyStartCCW = key - 1;
                    long keyEndCW = key + (1 << 32);
                    long keyEndCCW = key - (1 << 32);
                    // Set start and end edges
                    wingedEdge.startCWEdge = mapWingedEdges[keyStartCW];
                    wingedEdge.endCWEdge = mapWingedEdges[keyEndCW];
                    wingedEdge.startCCWEdge = mapWingedEdges[keyStartCCW];
                    wingedEdge.endCCWEdge = mapWingedEdges[keyEndCCW];
                    // Set start and the end
                    wingedEdge.startVertex.edge = wingedEdge;
                    wingedEdge.endVertex.edge = wingedEdge;
                }
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();
            Vector3[] vertices = new Vector3[this.edges.Count * 2];
            int[] quads = new int[this.faces.Count * 4];

            foreach (Face face in this.faces)
            {
                WingedEdge edge = face.edge;
                WingedEdge startEdge = edge;
                do
                {
                    vertices[edge.index * 2] = edge.startVertex.position;
                    vertices[edge.index * 2 + 1] = edge.endVertex.position;
                    quads[edge.index * 4] = edge.index * 2;
                    quads[edge.index * 4 + 1] = edge.index * 2 + 1;

                    edge = edge.startCWEdge;
                } while (edge != startEdge);
            }
            return faceVertexMesh;
        }
        
        public string ConvertToCSVFormat(string separator = "\t")
        {
            int tabSize = Mathf.Max(edges.Count, faces.Count, vertices.Count);

            List<string> strings = new List<string>(tabSize);

            // WingedEdges
            for(int i = 0 ; i < edges.Count ; i++)
            {
                WingedEdge we = edges[i];
                strings[i] += we.index + separator +
                    we.startVertex.index + ", " + we.endVertex.index + separator +
                    we.startCWEdge.index + ", " + we.endCWEdge.index + separator +
                    we.startCCWEdge.index + ", " + we.endCCWEdge.index + separator + separator;
            }

            // Faces
            for(int i = 0 ; i < faces.Count ; i++)
            {
                Face f = faces[i];
                strings[i] += f.index + separator +
                    f.edge.index + separator + separator;
            }

            // Vertex
            for(int i = 0 ; i < vertices.Count ; i++)
            {
                Vertex v = vertices[i];
                strings[i] += v.index + separator +
                    v.position.x.ToString("N03") + " " +
                    v.position.y.ToString("N03") + " " +
                    v.position.z.ToString("N03") + " " + separator +
                    v.edge.index;
            }

            string header = "WingedEdges" + separator + separator + separator + separator + separator + "Faces" + separator + separator + separator + "Vertices" + "\n"
                + "Index" + separator + "start+endVertex index" + separator + "CW index" + separator + "CCW index" + separator + separator
                + "Index" + separator + "WingedEdge index" + separator + separator
                + "Index" + separator + "Position" + separator + "WingedEdge index" + "\n";

            return header + string.Join("\n", strings);
        }

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces)
        {
            if (drawVertices)
            {
                foreach (Vertex vertex in this.vertices)
                {
                    Gizmos.DrawSphere(vertex.position, 0.1f);
                }
            }
            if (drawEdges)
            {
                foreach (WingedEdge edge in this.edges)
                {
                    Gizmos.DrawLine(edge.startVertex.position, edge.endVertex.position);
                }
            }
            if (drawFaces)
            {
                foreach (Face face in this.faces)
                {
                    WingedEdge edge = face.edge;
                    WingedEdge startEdge = edge;
                    do
                    {
                        Gizmos.DrawLine(edge.startVertex.position, edge.endVertex.position);
                        edge = edge.startCWEdge;
                    } while (edge != startEdge);
                }
            }
        }
    }
}