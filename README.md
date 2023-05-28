# 3D_Object_Generation

Membres :
Juan IVANOFF | Rémi DA SILVA | Valentin BAILLEUL

Fonctionnalités développées : 
- Modélisation Géométrique de plusieurs formes à partir de C#.
- Boîtes à outil mathématique pour HalfEdge permettant de récupérer les faces/ edges adjacentes à une vertice et toutes les edges / vertices d’une face.
- Utilisation de Catmull-Clark en HalfEdge pour arrondir les surfaces en multipliant le nombre de surfaces des formes géométriques (effet utilisé jusqu’à 4 fois).
- Fonction de choix permettant de choisir la forme à faire apparaître.

Scène Unity3D :
- Contient des objets généré selon la valeur sélectionnée dans le champ associé dans l’inspecteur (Strip, Grid, Grid normalisée, Box, Chips, Polygon régulier, Pacman, un diamant et un diamant troué), ainsi que leurs divisions par Catmull-Clark.

Limitations et bugs connus :
- Limitation liée à l’utilisation d’HalfEdge : on ne génère qu’une face sur deux. Vu du mauvais côté, l’objet apparaîtra invisible.

Répartition des tâches :
- Rémi : Génération des mesh, dev de la boîte à outil mathématiques, dev de Catmull-Clark et dev des structures HalfEdge et WingedEdge.
- Juan : Recherches et développement sur l’algorithme de Catmull-Clark et dev de la fonction permettant de convertir un HalfEdge en VertexFace.
- Valentin : Génération des meshs pour les formes, dev de HalfEdge et de WingedEdge, fonctions annexes et debug.
