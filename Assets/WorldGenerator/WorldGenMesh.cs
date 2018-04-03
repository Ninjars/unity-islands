using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
	public class WorldGenMesh {
		private const int vertexLimit = 63000;
		
		public static void triangulate(GameObject gameObject, Material material, List<Center> centers, float size, float verticalScale) {
			List<int> indices = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			List<Center> sortedCenters = new List<Center>(centers);
			sortedCenters.Sort((a, b) => {
				bool xLess = a.coord.x < b.coord.x;
				return xLess || (xLess && a.coord.y < b.coord.y) ? -1 : 1;
			});
			foreach (Center center in sortedCenters) {
				addTrianglesForCenter(center, indices, vertices, colors, verticalScale);
				if (vertices.Count > vertexLimit) {
					addMeshSubObject(gameObject, material, size, indices, vertices, colors);
				}
			}
			if (vertices.Count > 0) {
				addMeshSubObject(gameObject, material, size,  indices, vertices, colors);
			}
		}
		public static void triangulate(GameObject gameObject, Material material, List<Center> centers, List<Coord> coords, float size, float verticalScale) {
			List<int> indices = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			List<Center> processedCenters = new List<Center>();


for (int i = 0; i < centers.Count; i++) {
Debug.Log("coord " + coords[i]);
Debug.Log("center " + centers[i].coord);
}

			for (int i = 0; i < centers.Count; i++) {
				Center center = centers[i];
				Coord centerCoord = coords[i];
				List<Coord> neighbouringCoords = center.neighbours
														.Where(c => !processedCenters.Contains(c) && centers.Contains(c))
														.Select(c => coords[centers.IndexOf(c)])
														.ToList();
				if (neighbouringCoords.Count > 1) {
Debug.Log("neighbours");
foreach (Coord c in neighbouringCoords) {
	Debug.Log(" " + c);
}
					neighbouringCoords.Sort((a, b) => sortCoords(centerCoord, a, b) ? 1 : -1);
Debug.Log("sorted neighbours");
foreach (Coord c in neighbouringCoords) {
	Debug.Log(" " + c);
}
					addTrianglesForCoord(centerCoord, neighbouringCoords, indices, vertices, colors, verticalScale);
				}
				processedCenters.Add(center);
				// TODO: map between center and coord; should have the same ordering
				// TODO: Check for border corners to triangulate with coord
				// TODO: triangulate coord with neighbouring coords mapped from center.neighbours
				// TODO: work out how to consistently order triangulation D:

				if (vertices.Count > vertexLimit) {
					addMeshSubObject(gameObject, material, size, indices, vertices, colors);
				}
			}
			if (vertices.Count > 0) {
				addMeshSubObject(gameObject, material, size, indices, vertices, colors);
			}
		}

        private static void addTrianglesForCoord(Coord center,
											List<Coord> corners, 
											List<int> indices, 
											List<Vector3> vertices,
											List<Color> colors,
											float verticalScale) {
            Vector3 vertexCenter = center.toVector3(verticalScale);

            for (int i = 0; i < corners.Count; i++) {
				Coord corner1 = corners[i];
				int index2 = i + 1 >= corners.Count ? 0 : i + 1;
				Coord corner2 = corners[index2];

				Vector3 vertex1 = corner1.toVector3(verticalScale);
				Vector3 vertex2 = corner2.toVector3(verticalScale);
Debug.Log("vertex center " + vertexCenter);
Debug.Log("vertex 1 " + vertex1);
Debug.Log("vertex 2" + vertex2);
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

        private static void addMeshSubObject(GameObject containingObject, Material material, float worldSize, List<int> indices, List<Vector3> vertices, List<Color> colors) {
			GameObject gameObject = new GameObject();
			gameObject.name = "mesh section";
			gameObject.transform.SetParent(containingObject.transform);
			gameObject.transform.position = Vector3.zero;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
			gameObject.GetComponent<Renderer>().material = material;

			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.RecalculateNormals();
			Vector2[] myUVs = new Vector2[vertices.Count];
			for (var i = 0; i < vertices.Count; i++) {
				myUVs[i] = new Vector2(vertices[i].x / worldSize, vertices[i].y /  worldSize);
			}
			mesh.uv = myUVs;
			
			indices.Clear();
			vertices.Clear();
			colors.Clear();
		}

        private static void addTrianglesForCenter(Center center, 
											List<int> indices, 
											List<Vector3> vertices,
											List<Color> colors,
											float verticalScale) {
            Vector3 vertexCenter = center.coord.toVector3(verticalScale);
            Color color = getColor(center.terrainType);
			if (center.isClipped) {
				return;
			}

            List<Corner> corners = center.corners;
            for (int i = 0; i < corners.Count; i++) {
				Corner corner1 = corners[i];
				int index2 = i + 1 >= corners.Count ? 0 : i + 1;
				Corner corner2 = corners[index2];

				Vector3 vertex1 = corner1.coord.toVector3(verticalScale);
				Vector3 vertex2 = corner2.coord.toVector3(verticalScale);
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
