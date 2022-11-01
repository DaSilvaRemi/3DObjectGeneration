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

        public int nVerticesForTopology;

        public HalfEdgeMesh(Mesh mesh)
        {
            // constructeur prenant un mesh Vertex-Face en paramètre
            this.vertices = new List<Vertex>();
            this.edges = new List<HalfEdge>();
            this.faces = new List<Face>();
            Vector3[] meshVertices = mesh.vertices;

            //List<string> listOfIndex = new List<string>();
            Dictionary<string, int> mapOfIndex = new Dictionary<string, int>();

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                this.vertices.Add(new Vertex(i, meshVertices[i]));
            }

            int[] shapes = mesh.GetIndices(0);

            MeshTopology meshTopology = mesh.GetTopology(0);
            this.nVerticesForTopology = meshTopology.Equals(MeshTopology.Triangles) ? 3 : 4;

            int indexVertex = 0;
            int indexHalfEdge = 0;
            int nbFaces = shapes.Length / nVerticesForTopology;

            int cmp = 0;

            for (int i = 0; i < nbFaces; i++)
            {
                Face f = new Face(i);
                List<HalfEdge> tempHalfEdges = new List<HalfEdge>();

                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    Vertex v = this.vertices[shapes[indexVertex++]];
                    HalfEdge he = new HalfEdge(indexHalfEdge++, v, f);
                    tempHalfEdges.Add(he);


                    //long startIndex = indexVertex;
                    //long endIndex = (indexVertex < nVerticesForTopology - 1) ? indexVertex : 0;
                    //long key = startIndex + (endIndex << 32);
                    //mapHalfEdges.Add(key, he);
                }

                int nbTempHalfEdge = tempHalfEdges.Count;

                for (int j = 0; j < nbTempHalfEdge; j++)
                {
                    HalfEdge currentHE = tempHalfEdges[j];
                    int nextEdgeIndice = (j == nbTempHalfEdge - 1) ? 0 : j + 1;
                    int previousEdgeIndice = (j == 0) ? nbTempHalfEdge - 1 : j - 1;

                    currentHE.prevEdge = tempHalfEdges[previousEdgeIndice];
                    currentHE.nextEdge = tempHalfEdges[nextEdgeIndice];


                    // // Modification de la twin edge si elle existe
                    // HalfEdge he1;
                    // int startIndex = currentHE.sourceVertex.index;
                    // long endIndex = currentHE.nextEdge.sourceVertex.index;
                    // // long keyReversed = endIndex + (startIndex << 32);
                    // if (mapHalfEdges.TryGetValue(keyReversed, out he))
                    // {
                    //     // Imaginons 0->1 et sa twin 1->0
                    //     // La keyReversed de currentHE sera donc 1 + 0<<32 (pseudo-code)
                    //     // Si cette HalfEdge existe, ça veut dire qu'on a trouvé la twin de notre currentHE
                    //     // On lui applique donc sa twin.
                    //     // Pour ce qui est de la twin de la twin, elle sera appliquée dans une autre itération de la boucle for
                    //     currentHE.twinEdge = he;
                    // }

                    currentHE.sourceVertex.outgoingEdge = currentHE;
                    this.edges.Add(currentHE);
                }

                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    int startIndex = this.edges[cmp].sourceVertex.index;
                    int endIndex = this.edges[cmp].nextEdge.sourceVertex.index;//(((compteur + 1) % nVerticesForTopology) != 0) ? compteur + 1 : (compteur - nVerticesForTopology + 1);
                    // Debug.Log("Test: " + startIndex + " : " + endIndex);
                    string newKey = startIndex + "|" + endIndex;
                    //listOfIndex.Add(newKey);
                    mapOfIndex.Add(newKey, this.edges[cmp].index);

                    cmp++;
                }
                f.edge = tempHalfEdges[0];
                this.faces.Add(f);
            }

            string myDebug = "";
            foreach(KeyValuePair<string, int> kp in mapOfIndex)
            {
                string key = kp.Key;
                int value = kp.Value;
                //Debug.Log("List of index : " + listOfIndex.ToString());
                int startIndex = int.Parse(key.Split("|")[0]);
                int endIndex = int.Parse(key.Split("|")[1]);
                // Debug.Log("" + startIndex + " : " + endIndex);
                string reversedKey = "" + endIndex + "|" + startIndex;

                myDebug += key + " => " + value +   "\n";
                int reversedValue;
                if(mapOfIndex.TryGetValue(reversedKey, out reversedValue)){


                    this.edges[reversedValue].twinEdge = this.edges[value];
                    this.edges[value].twinEdge = this.edges[reversedValue];
                }

                // int out_start;
                // // int out_end;
                // if(listOfIndex.TryGetValue(endIndex, out out_start)){
                //     if(out_start != startIndex)
                //         continue;

                //     // écrire ici
                //     this.edges[endIndex].twinEdge = this.edges[out_start];
                //     this.edges[out_start].twinEdge = this.edges[endIndex];
                // }
            }

            Debug.Log(myDebug);
        }
        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh newMesh = new Mesh();

            Vector3[] vertices = new Vector3[this.vertices.Count];
            int[] quads = new int[faces.Count * this.nVerticesForTopology];

            foreach (Vertex v in this.vertices)
            {
                vertices[v.index] = v.position;
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Face f = faces[i];
                HalfEdge he = f.edge.prevEdge;
                int offset = 0;
                int j = i * this.nVerticesForTopology;
                do
                {
                    quads[j + offset] = he.sourceVertex.index;
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
            int tabSize = Mathf.Max(edges.Count, faces.Count, vertices.Count);
            List<string> strings = new List<string>(new string[tabSize]);
            Debug.Log("edges.Count = " + edges.Count);
            Debug.Log("faces.Count = " + faces.Count);
            Debug.Log("vertices.Count = " + vertices.Count);
            Debug.Log("tabSize = " + tabSize);
            Debug.Log("strings size = " + strings.Count);

            for (int i = 0; i < this.edges.Count; i++)
            {
                HalfEdge he = this.edges[i];
                strings[i] += he.index + separator;
                strings[i] += he.sourceVertex.index + "; " + he.nextEdge.sourceVertex.index + separator;
                strings[i] += he.prevEdge.index + "; " + he.nextEdge.index + "; ";
                strings[i] += he.twinEdge != null ? he.twinEdge.index : "null";
                strings[i] += separator + separator;
            }
            for (int i = edges.Count; i < tabSize; i++)
            {
                // Compléter les colonnes restantes par des separator
                strings[i] += separator + separator + separator + separator;
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Face f = faces[i];
                strings[i] += f.index + separator + f.edge.index + separator + separator;
            }

            for (int i = faces.Count; i < tabSize; i++)
            {
                // Compléter les colonnes restantes par des separator
                strings[i] += separator + separator + separator;
            }

            for (int i = 0; i < this.vertices.Count; i++)
            {
                Vertex vertex = this.vertices[i];
                Vector3 pos = vertex.position;
                strings[i] += vertex.index + separator;
                strings[i] += pos.x.ToString("N02") + " ";
                strings[i] += pos.y.ToString("N02") + " ";
                strings[i] += pos.z.ToString("N02") + separator;
                strings[i] += vertex.outgoingEdge.index + separator + separator;
            }

            string header = "HalfEdges" + separator + separator + separator + separator + "Faces" + separator + separator + separator + "Vertices\n" +
                "Index" + separator + "Src Vertex Index,end" + separator + "Prev + Next + Twin HalfEdge Index" + separator + separator +
                "Index" + separator + "HalfEdge Index" + separator + separator +
                "Index" + separator + "Position" + separator + "OutgoingEdgeIndex" + "\n";

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