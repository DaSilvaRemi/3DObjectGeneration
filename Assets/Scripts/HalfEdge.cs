using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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

        public HalfEdge() : this(0, null, null)
        {

        }

        public HalfEdge(int index, Vertex vertex, Face face)
        {
            this.index = index;
            this.sourceVertex = vertex;
            this.face = face;
        }
    }
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public HalfEdge outgoingEdge;

        public Vertex() : this(0, Vector3.zero)
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
        public HalfEdge edge;

        public Face() : this(0)
        {

        }

        public Face(int index)
        {
            this.index = index;
        }
    }
    public class HalfEdgeMesh
    {
        public List<Vertex> vertices;
        public List<HalfEdge> edges;
        public List<Face> faces;
        public HalfEdgeMesh(Mesh mesh)
        {
            // constructeur prenant un mesh Vertex-Face en paramètre
            //magic happens
            this.vertices = new List<Vertex>();
            this.edges = new List<HalfEdge>();
            this.faces = new List<Face>();
            Vector3[] meshVertices = mesh.vertices;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                this.vertices.Add(new Vertex(i, meshVertices[i]));
            }

            int[] shapes = mesh.GetIndices(0);

            MeshTopology meshTopology = mesh.GetTopology(0);
            
            int nVerticesForTopology = meshTopology.Equals(MeshTopology.Triangles) ? 3 : 4;

            for (int i = 0; i < shapes.Length / nVerticesForTopology; i++)
            {
                Face f = new Face(i);
                List<HalfEdge> tempHalfEdges = new List<HalfEdge>();

                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    Vertex v = this.vertices[shapes[j]];
                    HalfEdge hf = new HalfEdge(j, v, f);
                    tempHalfEdges.Add(hf);
                }

                for (int j = 0; j < tempHalfEdges.Count; j++)
                {
                    HalfEdge currentHF = tempHalfEdges[j];
                    int nextEdgeIndice = j == tempHalfEdges.Count - 1 ? 0 : j + 1; 
                    int previousEdgeIndice = j == 0 ? tempHalfEdges.Count - 1 : j - 1;

                    currentHF.prevEdge = tempHalfEdges[previousEdgeIndice];
                    currentHF.nextEdge = tempHalfEdges[nextEdgeIndice];
                    currentHF.sourceVertex.outgoingEdge = currentHF;
                    this.edges.Add(currentHF);
                }

                f.edge = tempHalfEdges[0];
                this.faces.Add(f);

                /*Vertex v0 = this.vertices[shapes[indexVertex]];
                HalfEdge hf_0 = new HalfEdge(indexHalfEdge++, v0, f);
                indexVertex++;

                Vertex v1 = this.vertices[shapes[indexVertex]];
                HalfEdge hf_1 = new HalfEdge(indexHalfEdge++, v1, f);
                indexVertex++;

                Vertex v2 = this.vertices[shapes[indexVertex]];
                HalfEdge hf_2 = new HalfEdge(indexHalfEdge++, v2, f);
                indexVertex++;

                Vertex v3 = this.vertices[shapes[indexVertex]];
                HalfEdge hf_3 = new HalfEdge(indexHalfEdge++, v3, f);
                indexVertex++;*/

                /*v0.outgoingEdge = hf_0;
                v1.outgoingEdge = hf_1;
                v2.outgoingEdge = hf_2;
                v3.outgoingEdge = hf_3;*/



                /*hf_0.prevEdge = hf_3;
                hf_0.nextEdge = hf_1;

                hf_1.prevEdge = hf_0;
                hf_1.nextEdge = hf_2;

                hf_2.prevEdge = hf_1;
                hf_2.nextEdge = hf_3;

                hf_3.prevEdge = hf_2;
                hf_3.nextEdge = hf_0;*/

                /*this.edges.Add(hf_0);
                this.edges.Add(hf_1);
                this.edges.Add(hf_2);
                this.edges.Add(hf_3);*/


            }
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