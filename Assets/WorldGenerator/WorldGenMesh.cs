using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
    public class WorldGenMesh {
        private const int vertexLimit = 63000;

        public static void triangulate(GameObject gameObject, Material material, List<Center> centers, float size) {
            List<int> indices = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<Center> sortedCenters = new List<Center>(centers);
            sortedCenters.Sort((a, b) => {
                bool xLess = a.coord.x < b.coord.x;
                return xLess || (xLess && a.coord.y < b.coord.y) ? -1 : 1;
            });
            foreach (Center center in sortedCenters) {
                addTrianglesForCenter(center, indices, vertices, colors);
                if (vertices.Count > vertexLimit) {
                    createMeshSubObject(gameObject, material, size, indices, vertices, colors);
                }
            }
            if (vertices.Count > 0) {
                createMeshSubObject(gameObject, material, size, indices, vertices, colors);
            }
        }
        public static void triangulate(GameObject gameObject, Material material, List<ConnectedCoord> coords, float size) {
            List<int> indices = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<CoordUnderside> processedCenters = new List<CoordUnderside>();

            foreach (ConnectedCoord coord in coords) {
                CoordUnderside centerCoord = coord.coord;
                List<CoordUnderside> neighbouringCoords = coord.neighbours.OrderBy(c => Math.Atan2(centerCoord.x - c.x, centerCoord.y - c.y)).ToList();
                if (neighbouringCoords.Count > 1) {
                    addTrianglesForCoord(centerCoord, neighbouringCoords, indices, vertices, colors);
                }

                processedCenters.Add(centerCoord);

                if (vertices.Count > vertexLimit) {
                    createMeshSubObject(gameObject, material, size, indices, vertices, colors);
                }
            }
            if (vertices.Count > 0) {
                createMeshSubObject(gameObject, material, size, indices, vertices, colors);
            }
        }

        private static void addTrianglesForCoord(CoordUnderside center,
                                            List<CoordUnderside> corners,
                                            List<int> indices,
                                            List<Vector3> vertices,
                                            List<Color> colors) {
            Vector3 vertexCenter = center.toVector3(1);

            for (int i = 0; i < corners.Count; i++) {
                CoordUnderside corner1 = corners[i];
                int index2 = i + 1 >= corners.Count ? 0 : i + 1;
                CoordUnderside corner2 = corners[index2];

                Vector3 vertex1 = corner1.toVector3(1);
                Vector3 vertex2 = corner2.toVector3(1);
                addTriangle(vertexCenter, vertex1, vertex2, Color.grey, indices, vertices, colors);
            }
        }

        private static bool sortCoords(Coord center, Coord a, Coord b) {
            if (a.x - center.x >= 0 && b.x - center.x < 0)
                return true;
            if (a.x - center.x < 0 && b.x - center.x >= 0)
                return false;
            if (a.x - center.x == 0 && b.x - center.x == 0) {
                if (a.y - center.y >= 0 || b.y - center.y >= 0)
                    return a.y > b.y;
                return b.y > a.y;
            }

            // compute the cross product of vectors (center -> a) x (center -> b)
            float det = (a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y);
            if (det < 0)
                return true;
            if (det > 0)
                return false;

            // points a and b are on the same line from the center
            // check which point is closer to the center
            float d1 = (a.x - center.x) * (a.x - center.x) + (a.y - center.y) * (a.y - center.y);
            float d2 = (b.x - center.x) * (b.x - center.x) + (b.y - center.y) * (b.y - center.y);
            return d1 > d2;
        }

        private static GameObject createMeshSubObject(GameObject containingObject, Material material, float worldSize, List<int> indices, List<Vector3> vertices, List<Color> colors) {
            GameObject gameObject = new GameObject();
            gameObject.name = "mesh section";
            gameObject.layer = LayerMask.NameToLayer("Terrain");
            gameObject.transform.SetParent(containingObject.transform);
            gameObject.transform.position = Vector3.zero;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            gameObject.AddComponent<MeshRenderer>();
            gameObject.GetComponent<Renderer>().material = material;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.RecalculateNormals();
            Vector2[] myUVs = new Vector2[vertices.Count];
            for (var i = 0; i < vertices.Count; i++) {
                myUVs[i] = new Vector2(vertices[i].x / worldSize, vertices[i].y / worldSize);
            }
            mesh.uv = myUVs;

            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            indices.Clear();
            vertices.Clear();
            colors.Clear();

            return gameObject;
        }

        private static void addTrianglesForCenter(
            Center center,
            List<int> indices,
            List<Vector3> vertices,
            List<Color> colors
        ) {
            Vector3 vertexCenter = center.coord.toVector3(1);
            Color color = getColor(center.terrainType);
            if (center.isClipped) {
                return;
            }

            List<Corner> corners = center.corners;
            for (int i = 0; i < corners.Count; i++) {
                Corner corner1 = corners[i];
                int index2 = i + 1 >= corners.Count ? 0 : i + 1;
                Corner corner2 = corners[index2];

                Vector3 vertex1 = corner1.coord.toVector3(1);
                Vector3 vertex2 = corner2.coord.toVector3(1);
                addTriangle(vertexCenter, vertex1, vertex2, color, indices, vertices, colors);
            }
        }

        private static void addTriangle(Vector3 a, Vector3 b, Vector3 c, Color color,
                                    List<int> indices, List<Vector3> vertices, List<Color> colors) {
            int indicesOffset = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            indices.Add(indicesOffset);
            indices.Add(indicesOffset + 2);
            indices.Add(indicesOffset + 1);
        }

        private void assignVertexColors(World world, Mesh mesh) {
            Color[] colors = new Color[mesh.vertices.Length];
            foreach (Corner corner in world.corners) {
                foreach (int index in corner.vertexIndices) {
                    colors[index] = getColor(corner.terrainType);
                }
            }
            mesh.colors = colors;
        }

        private static Color getColor(TerrainType terrainType) {
            switch (terrainType) {
                case TerrainType.OCEAN:
                    return Color.blue;
                case TerrainType.LAKE:
                    return Color.cyan;
                case TerrainType.LAND:
                    return Color.grey;
                case TerrainType.COAST:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
    }
}
