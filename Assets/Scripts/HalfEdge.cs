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

            // Pour chaque face on créer son facepoint selon la moyenne de tous ces vertices
            foreach (Face face in this.faces)
            {
                Vector3 meanPos = Vector3.zero;

                for (int j = face.index; j < face.index + this.nVerticesForTopology; j++)
                {
                    meanPos += this.vertices[j].position;
                }

                meanPos /= this.nVerticesForTopology;
                facePoints.Add(meanPos);
            }

            // Pour chaque edge nous calculons la moyenne de la start vertex, end vertex et des faces points des faces adjacentes
            foreach (HalfEdge edge in this.edges)
            {
                HalfEdge twinEdge = edge.twinEdge;

                Vector3 edgeFacePoint = facePoints[edge.face.index];
                Vector3 edgeTwinFacePoint = twinEdge != null ? facePoints[twinEdge.face.index] : Vector3.zero;
                Vector3 srcVertexPos = edge.sourceVertex.position;
                Vector3 endVertexPos = edge.nextEdge.sourceVertex.position;

                // Calcul de l'edge point et du midPoint. Si l'edge est en bodure l'edge point vaudra midPoint
                Vector3 midPoint = (srcVertexPos + endVertexPos) / 2;
                Vector3 edgePoint = twinEdge == null ? midPoint : (edgeFacePoint + edgeTwinFacePoint + srcVertexPos + endVertexPos) / 4;

                edgePoints.Add(edgePoint);
                midPoints.Add(midPoint);
            }

            // On recalcule la position de chaque vertices
            foreach (Vertex v in this.vertices)
            {
                Dictionary<int, HalfEdge> adjacentEdges = new Dictionary<int, HalfEdge>();

                // On trouve tous les edges adjacents à V et on calcule le nombres d'edges incidents.
                int nbIncidentEdges = 0;
                foreach (HalfEdge edge in this.edges)
                {
                    if (edge.sourceVertex == v)
                    {
                        adjacentEdges.Add(edge.index, edge);
                        nbIncidentEdges++;
                    }
                    else if (edge.nextEdge.sourceVertex == v)
                    {
                        //adjacentEdges.Add(edge.index, edge.nextEdge);
                        nbIncidentEdges++;
                    }
                }

                // Calcul de la somme du nombre de face et des mids points adjacents à la vertices
                Vector3 facePointsSum = Vector3.zero;
                Vector3 midPointsSum = Vector3.zero;
                int nbFacesPointAdjacent = 0;
                int nbMidPointAdjacent = 0;
                foreach (KeyValuePair<int, HalfEdge> kp in adjacentEdges)
                {
                    HalfEdge halfEdge = kp.Value;
                    facePointsSum += facePoints[halfEdge.face.index];
                    midPointsSum += midPoints[halfEdge.index];
                    nbFacesPointAdjacent++;
                    nbMidPointAdjacent++;
                }

                Vector3 a;
                Vector3 b = Vector3.zero;
                Vector3 c = Vector3.zero;

                // Dans le cas où nous somme à l'intérieur on calcule Q et R et ensuite chacun des trois termes de l'équation
                if (v.outgoingEdge.twinEdge != null)
                {
                    Vector3 Q = facePointsSum / nbFacesPointAdjacent;
                    Vector3 R = midPointsSum / nbMidPointAdjacent;
                    Vector3 V = v.position;

                    if (nbIncidentEdges == 0)
                    {
                        nbIncidentEdges = 1;
                    }

                    a = (1 / nbIncidentEdges) * Q;
                    b = (2 / nbIncidentEdges) * R;
                    c = ((nbIncidentEdges - 3) / nbIncidentEdges) * V;
                }
                // Dans le cas où nous sommes en bordure nous calculons uniquement la moyenne des midspoints et de V
                else
                {
                    Vector3 R = midPointsSum + v.position;
                    a = R / (nbMidPointAdjacent + 1);
                }

                Vector3 newV = a + b + c;
                vertexPoints.Add(newV);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="splittingPoint"></param>
        public void SplitEdge(HalfEdge edge, Vector3 splittingPoint)
        {
            // On créer le edge point
            Vertex edgePointVertex = new Vertex(this.vertices.Count, splittingPoint);
            HalfEdge edgePoint = new HalfEdge(this.edges.Count, edgePointVertex, edge.face, edge, edge.nextEdge, edge.twinEdge);
            edgePointVertex.outgoingEdge = edgePoint;

            // On positionne correctement la nouvelle vertices créée
            this.vertices.Add(edgePointVertex);
            //for (int i = edgePointVertex.index; i < this.vertices.Count - 1; i++)
            //{
            //    this.vertices[i].index = i + 1;
            //    this.vertices[i + 1] = this.vertices[i];
            //}
            //this.vertices[edgePointVertex.index] = edgePointVertex;

            // On positionne correctement la nouvelle edge créee
            this.edges.Add(edgePoint);
            //for (int i = edgePoint.index + 1; i < this.edges.Count - 1; i++)
            //{
            //    this.edges[i].index = i + 1;
            //    this.edges[i + 1] = this.edges[i];
            //}
            //this.edges[edgePoint.index] = edgePoint;
            edge.nextEdge.prevEdge = edgePoint;
            edge.nextEdge = edgePoint;

            /*
                 
                       TO DO : AJOUTER LES TWIN SUR LES NOUVELLES EDGES
                 
            */
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

            int indexFace = this.faces.Count;
            int indexHalfEdge = this.edges.Count;
            int oldFacesSize = this.faces.Count;
            do
            {
                // Récupération des edges points
                HalfEdge prevEdge = currentEdge.prevEdge;
                HalfEdge nextEdge = currentEdge.nextEdge;

                // Création des nouvelles faces et des nouveaux edges
                Face currentFace = indexFace == oldFacesSize ? face : new Face(indexFace);

                HalfEdge nextEdgeToCenter = new HalfEdge(indexHalfEdge++, currentEdge.nextEdge.sourceVertex, currentFace, currentEdge, null, null);
                HalfEdge prevEdgeToCenter = new HalfEdge(indexHalfEdge++, facePointVertex, currentFace, nextEdgeToCenter, prevEdge, null); ;

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

                // Ajout des edges dans les edges du Mesh
                this.edges.Add(nextEdgeToCenter);
                this.edges.Add(prevEdgeToCenter);

                //Ajout des nouvelles edges dans un dictionnaire temporaire afin d'y configurer les twins
                int startIndex = prevEdgeToCenter.sourceVertex.index;
                int endIndex = prevEdgeToCenter.nextEdge.sourceVertex.index;
                string newKey = startIndex + "|" + endIndex;
                mapOfNewHalfEdgeCreated.Add(newKey, prevEdgeToCenter.index);

                startIndex = nextEdgeToCenter.sourceVertex.index;
                endIndex = nextEdgeToCenter.nextEdge.sourceVertex.index;
                newKey = startIndex + "|" + endIndex;
                mapOfNewHalfEdgeCreated.Add(newKey, nextEdgeToCenter.index);

                indexFace++;
                currentEdge = nextEdge.nextEdge;
            } while (firstHalfEdge != currentEdge);

            // Mise à jour des twin dans la liste des edges avec les edges précemment créé
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