using System.Collections.Generic;
using System.Linq;
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

        public HalfEdge() : this(0, null, null)
        {

        }

        public HalfEdge(int index, Vertex vertex, Face face) : this(index, vertex, face, null, null, null)
        {
        }

        public HalfEdge(int index, Vertex vertex, Face face, HalfEdge prevEdge, HalfEdge nextEdge, HalfEdge twinEdge)
        {
            this.index = index;
            this.sourceVertex = vertex;
            this.face = face;
            this.prevEdge = prevEdge;
            this.nextEdge = nextEdge;
            this.twinEdge = twinEdge;
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

        public Vertex(int index, Vector3 pos) : this(index, pos, null)
        {
        }

        public Vertex(int index, Vector3 pos, HalfEdge outgoingEdge)
        {
            this.index = index;
            this.position = pos;
            this.outgoingEdge = outgoingEdge;
        }
        public List<HalfEdge> GetAdjacentEdges(List<HalfEdge> edges)
        {
            List<HalfEdge> adjacentEdges = new List<HalfEdge>();

            // Pour chaque edge on récupère les edges adjacentes à la vertices
            foreach (HalfEdge edge in edges)
            {
                // Si l'edge actuel ou la next edge à comme source vertex la vertices alors elle est adjacente
                if (edge.sourceVertex == this || edge.nextEdge.sourceVertex == this)
                {
                    // Vérification si l'edge n'est pas déjà ajouté à la liste des edges adjacentes
                    if (!adjacentEdges.Contains(edge))
                    {
                        adjacentEdges.Add(edge);
                        adjacentEdges.Add(edge.twinEdge); // soit la twin, soit null
                    }
                }
            }
            return adjacentEdges;
        }

        public List<Face> GetAdjacentFaces(List<HalfEdge> edges)
        {
            Dictionary<int, Face> adjacentFaces = new Dictionary<int, Face>();

            List<HalfEdge> adjacentEdges = this.GetAdjacentEdges(edges);

            for (int i = 0; i < adjacentEdges.Count; i += 2)
            {
                HalfEdge edge = adjacentEdges[i];
                HalfEdge twinEdge = adjacentEdges[i + 1];
                adjacentFaces.TryAdd(edge.face.index, edge.face);

                if (twinEdge != null)
                {
                    adjacentFaces.TryAdd(twinEdge.face.index, twinEdge.face);
                }
            }
            return adjacentFaces.Values.ToList();
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

        public void GetEdgesVertices(out List<Vertex> faceVertices, out List<HalfEdge> faceEdges)
        {
            faceVertices = new List<Vertex>();
            faceEdges = new List<HalfEdge>();

            HalfEdge he = this.edge.prevEdge;
            do
            {
                faceVertices.Add(he.sourceVertex);
                faceEdges.Add(he);
                he = he.nextEdge;
            }
            while (this.edge.prevEdge != he);
        }

        public List<Vertex> GetVertices()
        {
            List<Vertex> faceVertices = new List<Vertex>();

            HalfEdge he = this.edge.prevEdge;
            do
            {
                faceVertices.Add(he.sourceVertex);
                he = he.nextEdge;
            }
            while (this.edge.prevEdge != he);

            return faceVertices;
        }

        public List<HalfEdge> GetEdges()
        {
            List<HalfEdge> faceEdges = new List<HalfEdge>();

            HalfEdge he = this.edge.prevEdge;
            do
            {
                faceEdges.Add(he);
                he = he.nextEdge;
            }
            while (this.edge.prevEdge != he);
            return faceEdges;
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
            foreach (KeyValuePair<string, int> kp in mapOfIndex)
            {
                string key = kp.Key;
                int value = kp.Value;
                //Debug.Log("List of index : " + listOfIndex.ToString());
                int startIndex = int.Parse(key.Split("|")[0]);
                int endIndex = int.Parse(key.Split("|")[1]);
                // Debug.Log("" + startIndex + " : " + endIndex);
                string reversedKey = "" + endIndex + "|" + startIndex;

                myDebug += key + " => " + value + "\n";
                int reversedValue;
                if (mapOfIndex.TryGetValue(reversedKey, out reversedValue))
                {


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

            //Debug.Log(myDebug);
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
            // Debug.Log("edges.Count = " + edges.Count);
            // Debug.Log("faces.Count = " + faces.Count);
            // Debug.Log("vertices.Count = " + vertices.Count);
            // Debug.Log("tabSize = " + tabSize);
            // Debug.Log("strings size = " + strings.Count);

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
                    Gizmos.DrawSphere(worldPos, 0.1f);
                    Handles.Label(worldPos, i.ToString(), style);
                }
            }

            style.normal.textColor = Color.black;

            if (drawEdges)
            {
                for (int i = 0; i < this.edges.Count; i++)
                {
                    Vector3 worldPosStart = transform.TransformPoint(this.edges[i].sourceVertex.position);
                    Vector3 worldPosEnd = transform.TransformPoint(this.edges[i].nextEdge.sourceVertex.position);
                    Gizmos.DrawLine(worldPosStart, worldPosEnd);
                    Handles.Label(worldPosEnd - worldPosStart / 2, "E : " + i, style);
                }
            }

            style.normal.textColor = Color.blue;

            if (drawFaces)
            {
                for (int i = 0; i < this.faces.Count; i++)
                {
                    HalfEdge e = this.faces[i].edge;
                    Vector3 p0 = e.sourceVertex.position;
                    Vector3 p1 = e.nextEdge.sourceVertex.position;
                    Vector3 p2 = e.nextEdge.nextEdge.sourceVertex.position;
                    Vector3 p3 = this.nVerticesForTopology > 3 ? e.nextEdge.nextEdge.nextEdge.sourceVertex.position : Vector3.zero;

                    int index1 = e.index;
                    int index2 = e.nextEdge.index;
                    int index3 = e.nextEdge.nextEdge.index;
                    int index4 = this.nVerticesForTopology > 3 ? e.nextEdge.nextEdge.nextEdge.index : -1;

                    string str = string.Format("{0} ({1},{2},{3},{4})", i, index1, index2, index3, index4);
                    Handles.Label((p0 + p1 + p2 + p3) / this.nVerticesForTopology, str, style);
                }
            }
        }

        public void SubdivideCatmullClark(int nbSubDiv)
        {
            for (int i = 0; i < nbSubDiv; i++)
            {
                this.SubdivideCatmullClark();
            }
        }

        public void SubdivideCatmullClark()
        {
            List<Vector3> facePoints;
            List<Vector3> edgePoints;
            List<Vector3> vertexPoints;

            this.CatmullClarkCreateNewPoints(out facePoints, out edgePoints, out vertexPoints);

            // Mise à jour des nouvelles positions
            for (int i = 0; i < vertexPoints.Count; i++)
            {
                this.vertices[i].position = vertexPoints[i];
            }

            // Split des edges
            for (int i = 0; i < edgePoints.Count; i++)
            {
                HalfEdge edge = this.edges[i];
                this.SplitEdge(edge, edgePoints[i]);
            }

            // Split des faces
            for (int i = 0; i < facePoints.Count; i++)
            {
                Face face = this.faces[i];
                this.SplitFace(face, facePoints[i]);
            }

            // Remise en place de toutes les vertices, edges et faces dans le tableau
            //this.vertices.OrderBy(v => v.index);
            //this.faces.OrderBy(f => f.index);
            //this.edges.OrderBy(e => e.index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facePoints"></param>
        /// <param name="edgePoints"></param>
        /// <param name="vertexPoints"></param>
        public void CatmullClarkCreateNewPoints(out List<Vector3> facePoints, out List<Vector3> edgePoints, out List<Vector3> vertexPoints)
        {
            facePoints = new List<Vector3>();
            edgePoints = new List<Vector3>();
            vertexPoints = new List<Vector3>();
            List<Vector3> midPoints = new List<Vector3>();

            // Pour chaque face on crée son facepoint selon la moyenne de toutes ses vertices
            for (int i = 0; i < this.faces.Count; i++)
            {
                List<Vertex> facesVertices = this.faces[i].GetVertices();
                Vector3 mean = Vector3.zero;

                // Somme de la position de tous les vertices
                for (int j = 0; j < facesVertices.Count; j++)
                {
                    mean += facesVertices[j].position;
                }

                mean /= (float)facesVertices.Count;
                facePoints.Add(mean);

            }

            // Pour chaque edge calcul de son "edgepoint" et de son "midpoint"
            for (int i = 0; i < this.edges.Count; i++)
            {
                HalfEdge edge = this.edges[i];
                Vector3 Vstart = edge.sourceVertex.position;
                Vector3 Vend = edge.nextEdge.sourceVertex.position;

                // Midpoint
                Vector3 midPoint = (Vstart + Vend) / 2;

                if (!midPoints.Contains(midPoint))
                {
                    midPoints.Add(midPoint);
                }

                // Edgepoint
                Vector3 mean = midPoint; // par défaut (sera utilisé si bordure)

                // Si on est pas en borduresnous calculons l'edge point
                if (edge.twinEdge != null)
                {
                    int indexC0 = edge.face.index;
                    int indexC1 = edge.twinEdge.face.index;

                    Vector3 C0 = facePoints[indexC0];
                    Vector3 C1 = facePoints[indexC1];

                    // On écrase le mean par la formule à appliquer en cas de non-bordure
                    mean = (Vstart + Vend + C0 + C1) / 4;
                }

                if (!edgePoints.Contains(mean))
                {
                    edgePoints.Add(mean);
                }
            }

            // Pour chaque vertices on récupère les faces adjacentes et les edges adjacentes
            for (int i = 0; i < this.vertices.Count; i++)
            {
                Vertex v = this.vertices[i];
                List<Face> adjacentFaces = v.GetAdjacentFaces(this.edges);
                List<HalfEdge> adjacentEdges = v.GetAdjacentEdges(this.edges);

                Vector3 meanFacePoint = Vector3.zero;
                Vector3 meanMidPoint = Vector3.zero;

                // Debug.Log("adjacentFaces.Count = " + adjacentFaces.Count + " ; adjacentEdges.Count = " + adjacentEdges.Count);
                // Debug.Log("facePoints.Count = " + facePoints.Count + " ; midPoints.Count = " + midPoints.Count);

                // Debug.Log("facePoints :");
                // foreach (var item in facePoints){
                //     Debug.Log(item);
                // }

                // Debug.Log("midPoints :");
                // foreach (var item in midPoints){
                //     Debug.Log(item);
                // }

                // On calcule la somme des facesPoints adjacents
                for (int j = 0; j < adjacentFaces.Count; j++)
                {
                    Face f = adjacentFaces[j];
                    meanFacePoint += facePoints[f.index];
                }

                // On calcule la somme des edgesPoints incidents 
                for (int j = 0; j < adjacentEdges.Count; j += 2)
                {
                    // HalfEdge e = adjacentEdges[j];
                    // meanMidPoint += midPoints[e.index];

                    HalfEdge edge = adjacentEdges[j];
                    Vector3 Vstart = edge.sourceVertex.position;
                    Vector3 Vend = edge.nextEdge.sourceVertex.position;

                    // Midpoint
                    Vector3 midPoint = (Vstart + Vend) / 2;
                    meanMidPoint += midPoint;
                    // Debug.Log(meanFacePoint);
                }

                Vector3 V = Vector3.zero;
                // Si on n'a aucune edge en bordure alors on est à l'intérieur donc on calcul V avec Q, R et V
                if (!adjacentEdges.Contains(null))
                {
                    int n = (adjacentEdges.Count / 2);
                    Vector3 Q = meanFacePoint / (float)adjacentFaces.Count;
                    Vector3 R = meanMidPoint / (float)n;
                    Vector3 V_before = v.position;

                    V = (1.0f / n) * Q + (2.0f / n) * R + ((n - 3.0f) / (float)n) * V_before;
                }
                // Sinon on est en bordures alors V est égal à la moyenne  des midspoints et de V
                else
                {
                    int nbBordures = 0;
                    Vector3 midPointSum = Vector3.zero;

                    for (int j = 1; j < adjacentEdges.Count; j += 2)
                    {
                        HalfEdge twin = adjacentEdges[j];
                        HalfEdge edge = adjacentEdges[j - 1];

                        if (twin == null)
                        {
                            midPointSum += midPoints[edge.index];
                            nbBordures++;
                        }
                    }
                    V = midPointSum / (float)nbBordures;
                }

                vertexPoints.Add(V);
            }
        }

        /// <summary>
        /// This is a summary
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="splittingPoint"></param>
        public void SplitEdge(HalfEdge edge, Vector3 splittingPoint)
        {
            // On créer le edge point
            Vertex edgePointVertex = new Vertex(this.vertices.Count, splittingPoint);
            HalfEdge edgePoint = new HalfEdge(this.edges.Count, edgePointVertex, edge.face, edge, edge.nextEdge, null);
            edgePointVertex.outgoingEdge = edgePoint;

            this.vertices.Add(edgePointVertex);
            this.edges.Add(edgePoint);

            edge.nextEdge.prevEdge = edgePoint;
            edge.nextEdge = edgePoint;

            // Si on a une twin on créer la twin edge en même temps
            if (edge.twinEdge != null)
            {
                //edge.twinEdge.sourceVertex = edgePointVertex;
                //On créer la twin edge point
                HalfEdge twinEdgePoint = new HalfEdge(this.edges.Count, edgePointVertex, edge.twinEdge.face, edge.twinEdge, edge.twinEdge.nextEdge, edge);

                edge.twinEdge.nextEdge.prevEdge = twinEdgePoint;
                edge.twinEdge.nextEdge = twinEdgePoint;
                edgePoint.twinEdge = edge.twinEdge;

                this.edges.Add(twinEdgePoint);
                this.edges[edgePoint.index] = edgePoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <param name="splittingPoint"></param>
        public void SplitFace(Face face, Vector3 splittingPoint)
        {
            Vertex facePointVertex = new Vertex(this.vertices.Count, splittingPoint);

            HalfEdge firstHalfEdge = face.edge;
            HalfEdge currentEdge = firstHalfEdge;

            Dictionary<string, int> mapOfNewHalfEdgeCreated = new Dictionary<string, int>();

            int indexFace = this.faces.Count - 1;
            int indexHalfEdge = this.edges.Count;
            int oldFacesSize = this.faces.Count - 1;
            do
            {
                // Récupération des edges points
                HalfEdge prevEdge = currentEdge.prevEdge;
                HalfEdge nextEdge = currentEdge.nextEdge;

                // Création des nouvelles faces et des nouveaux edges
                Face currentFace = indexFace == oldFacesSize ? face : new Face(indexFace);

                HalfEdge nextEdgeToCenter = new HalfEdge(indexHalfEdge++, currentEdge.nextEdge.sourceVertex, currentFace, currentEdge, null, null);
                HalfEdge prevEdgeToCenter = new HalfEdge(indexHalfEdge++, facePointVertex, currentFace, nextEdgeToCenter, prevEdge, null);

                prevEdge.prevEdge = prevEdgeToCenter;
                currentEdge.nextEdge = nextEdgeToCenter;
                nextEdgeToCenter.nextEdge = prevEdgeToCenter;
                currentFace.edge = nextEdgeToCenter;
                facePointVertex.outgoingEdge = prevEdgeToCenter;

                // Ajout de la face et des nouvelles edges
                if (indexFace != oldFacesSize)
                {
                    this.faces.Add(currentFace);
                }


                //Ajout des nouvelles edges dans un dictionnaire temporaire afin d'y configurer les twins
                int startIndex = prevEdgeToCenter.sourceVertex.index;
                int endIndex = prevEdgeToCenter.nextEdge.sourceVertex.index;
                string newKey = startIndex + "|" + endIndex;
                mapOfNewHalfEdgeCreated.Add(newKey, prevEdgeToCenter.index);

                startIndex = nextEdgeToCenter.sourceVertex.index;
                endIndex = nextEdgeToCenter.nextEdge.sourceVertex.index;
                newKey = startIndex + "|" + endIndex;
                mapOfNewHalfEdgeCreated.Add(newKey, nextEdgeToCenter.index);

                // Ajout des edges dans les edges du Mesh
                this.edges.Add(nextEdgeToCenter);
                this.edges.Add(prevEdgeToCenter);

                currentEdge = nextEdge.nextEdge;
                indexFace++;
            } while (firstHalfEdge != currentEdge);

            // Mise à jour des twin dans la liste des edges avec les edges précédemment créé
            foreach (KeyValuePair<string, int> kp in mapOfNewHalfEdgeCreated)
            {
                string key = kp.Key;
                int value = kp.Value;
                int startIndex = int.Parse(key.Split("|")[0]);
                int endIndex = int.Parse(key.Split("|")[1]);
                string reversedKey = endIndex + "|" + startIndex;

                int twinIndex;
                if (mapOfNewHalfEdgeCreated.TryGetValue(reversedKey, out twinIndex))
                {
                    this.edges[twinIndex].twinEdge = this.edges[value];
                    this.edges[value].twinEdge = this.edges[twinIndex];
                }
            }

            // Ajout de la vertice face point au tableau des vertices du Mesh
            this.vertices.Add(facePointVertex);
        }
    }
}