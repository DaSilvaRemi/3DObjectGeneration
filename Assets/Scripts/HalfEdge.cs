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

        public HalfEdge(Vertex vertex, Face face)
        {
            //this.index = index;
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

        public Face(int index, HalfEdge _edge)
        {
            this.index = index;
            this.edge = _edge;
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
            // string str = "";
            List<string> strings = new List<string>();

            foreach (Vertex vertex in this.vertices)
            {
                Vector3 pos = vertex.position;
                strings.Add(vertex.index + separator +
                    pos.x.ToString("N02") + " " +
                    pos.y.ToString("N02") + " " +
                    pos.z.ToString("N02") + separator +
                    vertex.outgoingEdge.index + separator + separator);
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Face f = faces[i];
                strings[i] += f.index + separator + f.edge.index + separator + separator;
            }

            for (int i = 0; i < this.edges.Count; i++)
            {
                HalfEdge he = this.edges[i];
                strings[i] += he.index + separator + he.sourceVertex.index + separator +
                he.prevEdge.index + "," + he.nextEdge.index + "," + he.twinEdge + "," +
                separator + separator;
            }

            string header = "HalfEdges" + separator + separator + separator + separator + "Faces" +
                separator + separator + separator + "Vertices\n" +
                "index" + separator + "sourceVertexIndex" + separator + "edgesIndex" + separator + separator + "index" +
                separator + "position" + separator + "outgoingEdgeIndex" + "\n";

            return header + string.Join("\n", strings);
        }

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces)
        {
            if (drawVertices)
            {
                foreach (Vertex v in this.vertices)
                {
                    Gizmos.DrawSphere(v.position, 0.1f);
                }
            }
            if (drawEdges)
            {
                foreach (HalfEdge e in this.edges)
                {
                    Gizmos.DrawLine(e.sourceVertex.position, e.nextEdge.sourceVertex.position);
                }
            }
            if (drawFaces)
            {
                foreach (Face f in this.faces)
                {
                    HalfEdge e = f.edge;
                    Vector3 p0 = e.sourceVertex.position;
                    Vector3 p1 = e.nextEdge.sourceVertex.position;
                    Vector3 p2 = e.nextEdge.nextEdge.sourceVertex.position;
                    Vector3 p3 = e.nextEdge.nextEdge.nextEdge.sourceVertex.position;
                    Gizmos.DrawLine(p0, p1);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p3);
                    Gizmos.DrawLine(p3, p0);
                }
            }
        }

        /**
        * Subdivide HalfEdge using Catmull-Clark methods
        */
        public void Subdivide()
        {
            //Create new vertices
            List<Vertex> newVertices = new List<Vertex>();
            foreach (Vertex v in this.vertices)
            {
                newVertices.Add(v);
            }
            foreach (Face f in this.faces)
            {
                HalfEdge e = f.edge;
                Vector3 p0 = e.sourceVertex.position;
                Vector3 p1 = e.nextEdge.sourceVertex.position;
                Vector3 p2 = e.nextEdge.nextEdge.sourceVertex.position;
                Vector3 p3 = e.nextEdge.nextEdge.nextEdge.sourceVertex.position;
                Vector3 newVertexPosition = (p0 + p1 + p2 + p3) / 4;
                Vertex newVertex = new Vertex(newVertices.Count, newVertexPosition);
                newVertices.Add(newVertex);
            }
            foreach (HalfEdge e in this.edges)
            {
                Vector3 p0 = e.sourceVertex.position;
                Vector3 p1 = e.nextEdge.sourceVertex.position;
                Vector3 p2 = e.nextEdge.nextEdge.sourceVertex.position;
                Vector3 p3 = e.nextEdge.nextEdge.nextEdge.sourceVertex.position;
                Vector3 newVertexPosition = (p0 + p1 + p2 + p3) / 4;
                Vertex newVertex = new Vertex(newVertices.Count, newVertexPosition);
                newVertices.Add(newVertex);
            }
            this.vertices = newVertices;

            //Create new faces
            this.splitFaces();

            //Create new edges
            List<HalfEdge> newEdges = this.splitEdges();

            // Calculate the new position of the edges
            for (int i = 0; i < newEdges.Count; i++)
            {
                HalfEdge e = newEdges[i];
                Vertex v0 = e.sourceVertex;
                Vertex v1 = e.nextEdge.sourceVertex;
                Vertex v2 = e.nextEdge.nextEdge.sourceVertex;
                Vertex v3 = e.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v4 = e.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v5 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v6 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v7 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;

                e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge = v0.outgoingEdge;
                e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge = v1.outgoingEdge;
                e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge = v2.outgoingEdge;
                e.nextEdge.nextEdge.nextEdge.nextEdge = v3.outgoingEdge;
                e.nextEdge.nextEdge.nextEdge = v4.outgoingEdge;
                e.nextEdge.nextEdge = v5.outgoingEdge;
                e.nextEdge = v6.outgoingEdge;
                e = v7.outgoingEdge;
            }

            this.edges = newEdges;
        }

        private List<HalfEdge> splitEdges()
        {
            List<HalfEdge> newEdges = new List<HalfEdge>();
            foreach (HalfEdge e in this.edges)
            {
                Vertex v0 = e.sourceVertex;
                Vertex v1 = e.nextEdge.sourceVertex;
                Vertex v2 = e.nextEdge.nextEdge.sourceVertex;
                Vertex v3 = e.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v4 = e.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v5 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v6 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v7 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;

                HalfEdge e0 = new HalfEdge(v0, e.face);
                HalfEdge e1 = new HalfEdge(v1, e.face);
                HalfEdge e2 = new HalfEdge(v2, e.face);
                HalfEdge e3 = new HalfEdge(v3, e.face);
                HalfEdge e4 = new HalfEdge(v4, e.face);
                HalfEdge e5 = new HalfEdge(v5, e.face);
                HalfEdge e6 = new HalfEdge(v6, e.face);
                HalfEdge e7 = new HalfEdge(v7, e.face);

                e0.nextEdge = e1;
                e1.nextEdge = e2;
                e2.nextEdge = e3;
                e3.nextEdge = e4;
                e4.nextEdge = e5;
                e5.nextEdge = e6;
                e6.nextEdge = e7;
                e7.nextEdge = e0;

                newEdges.Add(e0);
                newEdges.Add(e1);
                newEdges.Add(e2);
                newEdges.Add(e3);
                newEdges.Add(e4);
                newEdges.Add(e5);
                newEdges.Add(e6);
                newEdges.Add(e7);
            }
            return newEdges;
        }

        private void splitFaces()
        {
            List<Face> newFaces = new List<Face>();
            foreach (Face f in this.faces)
            {
                HalfEdge e = f.edge;
                Vertex v0 = e.sourceVertex;
                Vertex v1 = e.nextEdge.sourceVertex;
                Vertex v2 = e.nextEdge.nextEdge.sourceVertex;
                Vertex v3 = e.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v4 = e.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v5 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v6 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;
                Vertex v7 = e.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.nextEdge.sourceVertex;


                Face f0 = new Face(newFaces.Count);
                Face f1 = new Face(newFaces.Count + 1);
                Face f2 = new Face(newFaces.Count + 2);
                Face f3 = new Face(newFaces.Count + 3);

                HalfEdge e0 = new HalfEdge(v0, f0);
                HalfEdge e1 = new HalfEdge(v1, f0);
                HalfEdge e2 = new HalfEdge(v4, f0);
                HalfEdge e3 = new HalfEdge(v7, f0);

                HalfEdge e4 = new HalfEdge(v1, f1);
                HalfEdge e5 = new HalfEdge(v2, f1);
                HalfEdge e6 = new HalfEdge(v5, f1);
                HalfEdge e7 = new HalfEdge(v4, f1);

                HalfEdge e8 = new HalfEdge(v2, f2);
                HalfEdge e9 = new HalfEdge(v3, f2);
                HalfEdge e10 = new HalfEdge(v6, f2);
                HalfEdge e11 = new HalfEdge(v5, f2);

                HalfEdge e12 = new HalfEdge(v3, f3);
                HalfEdge e13 = new HalfEdge(v0, f3);
                HalfEdge e14 = new HalfEdge(v7, f3);
                HalfEdge e15 = new HalfEdge(v6, f3);

                e0.nextEdge = e1;
                e1.nextEdge = e2;
                e2.nextEdge = e3;
                e3.nextEdge = e0;

                e4.nextEdge = e5;
                e5.nextEdge = e6;
                e6.nextEdge = e7;
                e7.nextEdge = e4;

                e8.nextEdge = e9;
                e9.nextEdge = e10;
                e10.nextEdge = e11;
                e11.nextEdge = e8;

                e12.nextEdge = e13;
                e13.nextEdge = e14;
                e14.nextEdge = e15;
                e15.nextEdge = e12;

                f0.edge = e0;
                f1.edge = e4;
                f2.edge = e8;
                f3.edge = e12;

                newFaces.Add(f0);
                newFaces.Add(f1);
                newFaces.Add(f2);
                newFaces.Add(f3);
            }
            this.faces = newFaces;
        }
    }
}