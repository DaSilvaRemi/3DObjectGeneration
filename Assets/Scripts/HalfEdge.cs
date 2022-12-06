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

            // Pour chaque edge on récupère les edges adjacentes à la vertice
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

        public List<HalfEdge> GetIncidentEdges(List<HalfEdge> edges)
        {
            List<HalfEdge> incidentEdges = new List<HalfEdge>();

            // Pour chaque edge on récupère les edges adjacentes à la vertice
            foreach (HalfEdge edge in edges)
            {
                // Si l'edge actuel ou la next edge à comme source vertex la vertices alors elle est adjacente
                if (edge.nextEdge.sourceVertex == this && this.outgoingEdge != edge)
                {
                    // Vérification si l'edge n'est pas déjà ajouté à la liste des edges adjacentes
                    if (!incidentEdges.Contains(edge))
                    {
                        incidentEdges.Add(edge);
                    }
                }
            }
            return incidentEdges;
        }

        //public List<HalfEdge> GetAdjacentEdges(List<HalfEdge> edges)
        //{
        //    List<HalfEdge> adjacentEdges = new List<HalfEdge>();

        //    // Pour chaque edge on récupère les edges adjacentes à la vertice
        //    foreach (HalfEdge edge in edges)
        //    {
        //        // Si l'edge actuel ou la next edge à comme source vertex la vertices alors elle est adjacente
        //        if (edge.sourceVertex == this && !adjacentEdges.Contains(edge))
        //        {
        //            adjacentEdges.Add(edge);
        //            adjacentEdges.Add(edge.twinEdge); // soit la twin, soit null
        //        }
        //    }
        //    return adjacentEdges;
        //}

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

        //public List<Face> GetAdjacentFaces(List<HalfEdge> edges)
        //{
        //    //Dictionary<int, Face> adjacentFaces = new Dictionary<int, Face>();
        //    List<Face> adjacentFaces = new List<Face>();

        //    List<HalfEdge> adjacentEdges = this.GetAdjacentEdges(edges);

        //    for (int i = 0; i < adjacentEdges.Count; i += 2)
        //    {
        //        HalfEdge edge = adjacentEdges[i];

        //        if (edge.sourceVertex == this)
        //        {
        //            adjacentFaces.Add(edge.face);
        //        }
        //    }
        //    return adjacentFaces;
        //}
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

            HalfEdge he = this.edge;
            do
            {
                faceVertices.Add(he.sourceVertex);
                faceEdges.Add(he);
                he = he.nextEdge;
            }
            while (this.edge != he);
        }

        public List<Vertex> GetVertices()
        {
            List<Vertex> faceVertices = new List<Vertex>();

            HalfEdge he = this.edge;
            do
            {
                faceVertices.Add(he.sourceVertex);
                he = he.nextEdge;
            }
            while (this.edge != he);

            return faceVertices;
        }

        public List<HalfEdge> GetEdges()
        {
            List<HalfEdge> faceEdges = new List<HalfEdge>();

            HalfEdge he = this.edge;
            do
            {
                faceEdges.Add(he);
                he = he.nextEdge;
            }
            while (this.edge != he);
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
                    int endIndex = this.edges[cmp].nextEdge.sourceVertex.index;
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

        /// <summary>
        /// Récupère tout les edges physiques
        /// 
        /// C'est à dire que chaque edge est unique et ne comporte donc pas de twin
        /// </summary>
        public List<HalfEdge> GetPhysicalEdges()
        {
            Dictionary<int, HalfEdge> physicalEdges = new Dictionary<int, HalfEdge>();

            for (int i = 0; i < this.edges.Count; i++)
            {
                HalfEdge currentHalfEdge = this.edges[i];

                bool isExist = physicalEdges.ContainsKey(currentHalfEdge.index);

                if (currentHalfEdge.twinEdge != null)
                {
                    isExist |= physicalEdges.ContainsKey(currentHalfEdge.twinEdge.index);
                }

                if (!isExist)
                {
                    physicalEdges.Add(currentHalfEdge.index, currentHalfEdge);
                }
            }

            return physicalEdges.Values.ToList();
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
                    Handles.Label((worldPosEnd + worldPosStart) / 2, "E : " + i, style);
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
            for (int i = 0; i < this.vertices.Count; i++)
            {
                Vertex v = this.vertices[i];
                v.position = vertexPoints[v.index];
            }

            // Split des edges
            List<HalfEdge> edgesCopy = new List<HalfEdge>(this.edges);
            for (int i = 0; i < edgesCopy.Count; i++)
            {
                HalfEdge edge = edgesCopy[i];
                this.SplitEdge(edge, edgePoints[edge.index]);
            }

            // Split des faces
            List<Face> faceCopy = new List<Face>(this.faces);
            for (int i = 0; i < faceCopy.Count; i++)
            {
                Face face = faceCopy[i];
                this.SplitFace(face, facePoints[face.index]);
            }
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

                midPoints.Add(midPoint);

                // Edgepoint
                Vector3 mean = midPoint; // par défaut (sera utilisé si bordure)

                // Si on est pas en bordures nous calculons l'edge point
                if (edge.twinEdge != null)
                {
                    int indexC0 = edge.face.index;
                    int indexC1 = edge.twinEdge.face.index;

                    Vector3 C0 = facePoints[indexC0];
                    Vector3 C1 = facePoints[indexC1];

                    // On écrase le mean par la formule à appliquer en cas de non-bordure
                    mean = (Vstart + Vend + C0 + C1) / 4;
                }

                edgePoints.Add(mean);
            }

            // Pour chaque vertices on récupère les faces adjacentes et les edges adjacentes
            for (int i = 0; i < this.vertices.Count; i++)
            {
                Vertex currVert = this.vertices[i];
                List<Face> adjacentFaces = currVert.GetAdjacentFaces(this.edges);
                List<HalfEdge> adjacentEdges = currVert.GetAdjacentEdges(this.edges);
                List<HalfEdge> incidentEdges = currVert.GetIncidentEdges(this.edges);

                Vector3 meanFacePoint = Vector3.zero;
                Vector3 meanMidPoint = Vector3.zero;

                // On calcule la somme des facesPoints adjacents
                for (int j = 0; j < adjacentFaces.Count; j++)
                {
                    Face f = adjacentFaces[j];
                    meanFacePoint += facePoints[f.index];
                }

                // On calcule la somme des edgesPoints incidents 
                for (int j = 0; j < incidentEdges.Count; j++)
                {
                    HalfEdge edge = incidentEdges[j];
                    meanMidPoint += midPoints[edge.index];
                }

                Vector3 V = Vector3.zero;
                // Si on n'a aucune edge en bordure alors on est à l'intérieur donc on calcul V avec Q, R et V
                if (!adjacentEdges.Contains(null))
                {
                    int n = incidentEdges.Count;
                    Vector3 Q = meanFacePoint / (float)adjacentFaces.Count;
                    Vector3 R = meanMidPoint / (float)n;

                    V = ((1.0f * Q) / n) + ((2.0f * R) / n) + ((n - 3.0f) / (float)n) * currVert.position;
                }
                // Sinon on est en bordures alors V est égal à la moyenne  des midspoints et de V
                else
                {
                    Vector3 midPointSum = Vector3.zero;

                    for (int j = 1; j < adjacentEdges.Count; j += 2)
                    {
                        HalfEdge twin = adjacentEdges[j];
                        HalfEdge edge = adjacentEdges[j - 1];

                        if (twin == null)
                        {
                            midPointSum += midPoints[edge.index];
                        }
                    }
                    V = (midPointSum + currVert.position) / 3;
                }

                vertexPoints.Add(V);
            }
        }

        /// <summary>
        /// Split d'un edge avec son splittingPoint
        /// </summary>
        /// <param name="edge">L'edge qu'il faut split</param>
        /// <param name="splittingPoint">Le splitting point</param>
        public void SplitEdge(HalfEdge edge, Vector3 splittingPoint)
        {            
            // Vérification si l'edge point existe déjà
            bool edgePointIsAlreadyExist = edge.twinEdge != null 
                && (edge.twinEdge.sourceVertex != edge.nextEdge.sourceVertex || edge.twinEdge.nextEdge.sourceVertex != edge.sourceVertex);

            // Récupération ou création de la vertice + Création de la nouvelle edge
            Vertex edgePointVertex = edgePointIsAlreadyExist ? edge.twinEdge.nextEdge.sourceVertex : new Vertex(this.vertices.Count, splittingPoint);
            HalfEdge edgePoint = new HalfEdge(this.edges.Count, edgePointVertex, edge.face, edge, edge.nextEdge, null);
            
            edge.nextEdge.prevEdge = edgePoint;
            edge.nextEdge = edgePoint;

            // Si l'edge point n'existe pas alors on ajoute la vertice s
            if (!edgePointIsAlreadyExist)
            {
                edgePointVertex.outgoingEdge = edgePoint;
                this.vertices.Add(edgePointVertex);
            }
            // Sinon l'edge existe déjà alors on complète les twins
            else
            {
                HalfEdge nextTwinEdge = edge.twinEdge.nextEdge;

                nextTwinEdge.twinEdge = edge;
                edgePoint.twinEdge = edge.twinEdge;

                edge.twinEdge.twinEdge = edgePoint;
                edge.twinEdge = nextTwinEdge;
            }

            this.edges.Add(edgePoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <param name="splittingPoint"></param>
        public void SplitFace(Face face, Vector3 splittingPoint)
        {
            Vertex facePointVertex = new Vertex(this.vertices.Count, splittingPoint);
           
            HalfEdge edge = face.edge.nextEdge;
            HalfEdge lastNextToCenter = null;
            HalfEdge lastNextTwinToCenter = null;

            int indexHalfEdge = this.edges.Count;
            int indexFace = this.faces.Count - 1;
            int oldFaceCount = indexFace;
            
            do
            {
                // Récupération ou création de la face
                Face currentFace = indexFace == oldFaceCount ? face : new Face(indexFace, lastNextTwinToCenter);
                HalfEdge prevEdge = edge.prevEdge;
                HalfEdge nextEdge = edge.nextEdge;

                // Ajout de la face et complétion des paramètres manquant
                if (indexFace != oldFaceCount)
                {
                    this.faces.Add(currentFace);
                    lastNextTwinToCenter.face = currentFace; 
                }

                // Création des nouvelles edges
                HalfEdge nextEdgeToCenter = new HalfEdge(indexHalfEdge++, edge.sourceVertex, currentFace, prevEdge, lastNextTwinToCenter, null);
                HalfEdge twinNextEdgeToCenter = new HalfEdge(indexHalfEdge++, facePointVertex, null, null, edge, nextEdgeToCenter);
                
                if (lastNextTwinToCenter != null)
                {
                    lastNextTwinToCenter.prevEdge = nextEdgeToCenter;
                }

                // Paramétrage de l'edge précédente
                prevEdge.face = currentFace;
                prevEdge.prevEdge.face = currentFace;
                prevEdge.nextEdge = nextEdgeToCenter;

                // Assignation de la twin 
                lastNextTwinToCenter = twinNextEdgeToCenter;
                nextEdgeToCenter.twinEdge = twinNextEdgeToCenter;
                edge.prevEdge = twinNextEdgeToCenter;
                facePointVertex.outgoingEdge = twinNextEdgeToCenter;

                this.edges.Add(nextEdgeToCenter);
                this.edges.Add(twinNextEdgeToCenter);

                // Si la dernière edge créer allant vers le centre est null alors on l'assigne
                if (lastNextToCenter == null)
                {
                    lastNextToCenter = nextEdgeToCenter;
                }
                    
                ++indexFace;
                edge = nextEdge.nextEdge;
            } while (edge.prevEdge != face.edge);

            // Assignation des variables manquantes
            lastNextTwinToCenter.face = face;
            lastNextToCenter.nextEdge = lastNextTwinToCenter;
            lastNextTwinToCenter.prevEdge = lastNextToCenter;

            // Ajout de la vertice au Mesh
            this.vertices.Add(facePointVertex);
        }
    }
}