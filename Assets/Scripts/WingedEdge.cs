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

        public int nVerticesForTopology;

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

            this.nVerticesForTopology = meshTopology.Equals(MeshTopology.Triangles) ? 3 : 4;

            Dictionary<long, WingedEdge> mapWingedEdges = new Dictionary<long, WingedEdge>();


            // CREER les edges
            /*
            Itérer sur les faces
            Pour la face :
                Pour chaque edge :
                    Si non existante dans le dictionnaire :
                        Créer start, end, dire que E1 est la CCW de E0 et inversement
                        Ajouter edge dans le dictionnaire
                    Sinon :
                        Compléter la face réciproque de l'edge avec :
                            CW et CCW
                            leftFace <- face actuelle
                            Créer start, end, dire que E1 est la CCW de E0 et inversement
            */
            int nbFaces = shapes.Length / nVerticesForTopology;
            int indexVertex = 0;
            int indexWingedEdge = 0;
            // For each face
            for (int i = 0; i < nbFaces; i++)
            {
                Face currentFace = new Face(i);
                this.faces.Add(currentFace);
                Vertex[] faceVertex = new Vertex[nVerticesForTopology];


                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    Vertex v = this.vertices[shapes[indexVertex++]];
                    faceVertex[j] = v;
                }

                WingedEdge[] wingedEdges = new WingedEdge[nVerticesForTopology];

                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    Vertex start = faceVertex[j];
                    Vertex end = (j < nVerticesForTopology - 1) ? faceVertex[j + 1] : faceVertex[0];

                    // Check si edge existante dans le dictionnaire :
                    long min = Mathf.Min(start.index, end.index);
                    long max = Mathf.Max(start.index, end.index);

                    long key = min + (max << 32);

                    WingedEdge we;
                    if (!mapWingedEdges.TryGetValue(key, out we))
                    {
                        // Créer l'edge sans CW et CCW
                        we = new WingedEdge(indexWingedEdge++, start, end, null, currentFace);
                        mapWingedEdges.TryAdd(key, we);
                        this.edges.Add(we);
                        currentFace.edge = we;

                        start.edge = we;
                        // end.edge = we;
                    }
                    wingedEdges[j] = we;
                }

                for (int j = 0; j < nVerticesForTopology; j++)
                {
                    WingedEdge currentWingedEdge = wingedEdges[j];
                    if (!currentWingedEdge.rightFace.Equals(currentFace))
                    {
                        // Cas d'une edge déjà créée auparavant
                        currentWingedEdge.leftFace = currentFace;

                        // Nouvelles CW et CCW "complexes" :
                        currentWingedEdge.startCCWEdge = (j < nVerticesForTopology - 1) ? wingedEdges[j + 1] : wingedEdges[0];
                        currentWingedEdge.endCWEdge = (j == 0) ? wingedEdges[nVerticesForTopology - 1] : wingedEdges[j - 1];
                        currentWingedEdge.endVertex.edge = (j == 0) ? wingedEdges[nVerticesForTopology - 1] : wingedEdges[j - 1];
                    }
                    else
                    {
                        currentWingedEdge.rightFace = currentFace;

                        // Edges adjacentes "simples"
                        currentWingedEdge.startCWEdge = (j == 0) ? wingedEdges[nVerticesForTopology - 1] : wingedEdges[j - 1];
                        currentWingedEdge.endCCWEdge = (j < nVerticesForTopology - 1) ? wingedEdges[j + 1] : wingedEdges[0];
                        currentWingedEdge.endVertex.edge = (j < nVerticesForTopology - 1) ? wingedEdges[j + 1] : wingedEdges[0];
                    }
                }
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh newMesh = new Mesh();
            Vector3[] vertices = new Vector3[this.vertices.Count];
            int[] quads = new int[this.faces.Count * this.nVerticesForTopology];
            Debug.Log("Quad size : " + this.faces.Count * this.nVerticesForTopology);

            // Parcourir le tableau des Vertex et les mettres dans le tableau des vertices du Mesh
            for (int i = 0; i < this.vertices.Count; i++)
            {
                vertices[i] = this.vertices[i].position;
            }

            // Parcourir le tableau de faces récupérant son edge puis créer les Quads
            for (int i = 0; i < this.faces.Count; i++)
            {
                Face face = this.faces[i];
                WingedEdge current_edge = face.edge;
                WingedEdge firstEdge = current_edge;
                WingedEdge previousEdge = firstEdge;
                int offset = 0;
                int index = i * this.nVerticesForTopology;
                do
                {
                    //Debug.Log("Index : " + index + " Offset : " + offset + " Previous edge : " + previousEdge.index);
                    //Debug.Log("Start Vertex : " +  current_edge.startVertex.index);

                    // Récupération de l'indice de la vertice
                    int indiceVertex = (current_edge.endCWEdge != null && current_edge.endCWEdge.index == previousEdge.index)
                        ? current_edge.endVertex.index
                        : current_edge.startVertex.index;
                    //Debug.Log("Indice Vertex : " + indiceVertex);
                    quads[index + offset++] = indiceVertex;
                    WingedEdge tmp = current_edge;

                    // Récupération de la prochaine edge qu'on va parcourir
                    current_edge = (current_edge.endCWEdge != null && (current_edge.endCWEdge.index == firstEdge.index || current_edge.endCWEdge.index == previousEdge.index))
                        ? current_edge.startCCWEdge
                        : current_edge.endCCWEdge;
                    previousEdge = tmp;
                    //Debug.Log("Current CCW edge  : " + current_edge.index + " First edge end CCW " + firstEdge.endCCWEdge.index);
                } while (current_edge != firstEdge);
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


            // WingedEdges
            for (int i = 0; i < edges.Count; i++)
            {
                WingedEdge we = edges[i];
                strings[i] += we.index + separator;
                strings[i] += we.startVertex.index + ", " + we.endVertex.index + separator;


                /*
                // Edges adjacentes toujours existantes :
                startCWEdge
                endCCWEdge
                */

                strings[i] += we.startCWEdge.index + ", ";
                if (we.endCWEdge != null)
                {
                    strings[i] += we.endCWEdge.index;
                }
                else
                {
                    strings[i] += "null";
                }

                strings[i] += separator;

                if (we.startCCWEdge != null)
                {
                    strings[i] += we.startCCWEdge.index;
                }
                else
                {
                    strings[i] += "null";
                }
                strings[i] += ", " + we.endCCWEdge.index + separator + separator;
            }
            for (int i = edges.Count; i < tabSize; i++)
            {
                // Compléter les colonnes restantes par des separator (il peut y avoir ici des separator en trop)
                strings[i] += separator + separator + separator + separator + separator;
            }



            // Faces
            for (int i = 0; i < faces.Count; i++)
            {
                Face f = faces[i];
                strings[i] += f.index + separator;
                strings[i] += f.edge.index + separator + separator;
            }
            for (int i = faces.Count; i < tabSize; i++)
            {
                // Compléter les colonnes restantes par des separator
                strings[i] += separator + separator + separator;
            }



            // Vertex
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];
                strings[i] += v.index + separator;
                strings[i] += v.position.x.ToString("N03") + " ";
                strings[i] += v.position.y.ToString("N03") + " ";
                strings[i] += v.position.z.ToString("N03") + " " + separator;
                strings[i] += v.edge.index;
            }

            string header = "WingedEdges" + separator + separator + separator + separator + separator + "Faces" + separator + separator + separator + "Vertices" + "\n"
                + "Index" + separator + "start+endVertex index" + separator + "CW index" + separator + "CCW index" + separator + separator
                + "Index" + separator + "WingedEdge index" + separator + separator
                + "Index" + separator + "Position" + separator + "WingedEdge index" + "\n";

            return header + string.Join("\n", strings);
        }

        /// <summary>
        /// Dessine les vertices, edges, faces
        /// </summary>
        /// <param name="drawVertices">Booléean sur l'affichage des vertices</param>
        /// <param name="drawEdges">Booléean sur l'affichage des edges</param>
        /// <param name="drawFaces">Booléean sur l'affichage des faces</param>
        /// <param name="transform">Un objet transform pour calculer la position des points</param>
        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, Transform transform)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 15;
            style.normal.textColor = Color.red;


            if (drawVertices)
            {
                for (int i = 0; i < this.vertices.Count; i++)
                {
                    Vector3 worldPos = transform.TransformPoint(vertices[i].position);
                    Handles.Label(worldPos, i.ToString(), style);
                }
            }

            style.normal.textColor = Color.black;

            if (drawEdges)
            {
                for (int i = 0; i < this.edges.Count; i++)
                {
                    Vector3 worldPosStart = transform.TransformPoint(this.edges[i].startVertex.position);
                    Vector3 worldPosEnd = transform.TransformPoint(this.edges[i].endVertex.position);
                    Gizmos.DrawLine(worldPosStart, worldPosEnd);
                    Handles.Label((worldPosEnd + worldPosStart) / 2, "E : " + i, style);
                }
            }

            style.normal.textColor = Color.blue;

            if (drawFaces)
            {
                for (int i = 0; i < this.faces.Count; i++)
                {
                    Face face = this.faces[i];
                    WingedEdge current_edge = face.edge;
                    WingedEdge firstEdge = current_edge;
                    WingedEdge previousEdge = firstEdge;
                    int index = i * this.nVerticesForTopology;
                    string textToDisplay = "F" + i + " : (";
                    Vector3 sumPos = Vector3.zero;
                    do
                    {
                        int indiceVertex = (current_edge.endCWEdge != null && current_edge.endCWEdge.index == previousEdge.index)
                            ? current_edge.endVertex.index
                            : current_edge.startVertex.index;

                        textToDisplay += indiceVertex + " ";
                        sumPos += transform.TransformPoint(this.vertices[indiceVertex].position);
                        WingedEdge tmp = current_edge;

                        current_edge = (current_edge.endCWEdge != null && (current_edge.endCWEdge.index == firstEdge.index || current_edge.endCWEdge.index == previousEdge.index))
                            ? current_edge.startCCWEdge
                            : current_edge.endCCWEdge;
                        previousEdge = tmp;
                    } while (current_edge != firstEdge);

                    textToDisplay += ")";
                    Handles.Label(sumPos / this.nVerticesForTopology, textToDisplay);
                }
            }
        }
    }
}