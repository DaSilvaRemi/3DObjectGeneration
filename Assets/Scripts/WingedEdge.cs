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
            // constructeur prenant un mesh Vertex-Face en paramètre
            this.vertices = new List<Vertex>();
            this.edges = new List<WingedEdge>();
            this.faces = new List<Face>();
            Vector3[] meshVertices = mesh.vertices;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                this.vertices.Add(new Vertex(i, meshVertices[i]));
            }

            int[] shapes = mesh.GetIndices(0);

            MeshTopology meshTopology = mesh.GetTopology(0);
            
            int nVerticesForTopology = meshTopology.Equals(MeshTopology.Triangles) ? 3 : 4;
            int indexFace = 0;

            Dictionary<long, WingedEdge> mapWingedEdges = new Dictionary<long, WingedEdge>(); 

            for (int i = 0; i < shapes.Length / nVerticesForTopology; i++)
            { 
                Face leftFace = new Face(indexFace++);
                Face rightFace = new Face(indexFace++);

                for (int j = 0; j < nVerticesForTopology - 1; j += 2)
                {
                    Vertex vStart = this.vertices[shapes[j]];
                    Vertex vEnd = this.vertices[shapes[j + 1]];

                    WingedEdge wingedEdge = new WingedEdge(j, vStart, vEnd, leftFace, rightFace);

                    long key = j + ((j + 1) << 32); 
                    mapWingedEdges.Add(key, wingedEdge);
                }

                for (int j = 0; j < mapWingedEdges.Count; j++)
                {
                    KeyValuePair<long, WingedEdge> kp = mapWingedEdges.ElementAt(j);

                }
            }


        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();
            // magic happens
            return faceVertexMesh;
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