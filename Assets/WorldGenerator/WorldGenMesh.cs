using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class WorldGenMesh {
		private const int vertexLimit = 63000;
		
		public static void triangulate(GameObject gameObject, Material material, World world, float verticalScale) {
			foreach (Island island in world.islands) {
				List<int> indices = new List<int>();
				List<Vector3> vertices = new List<Vector3>();
				List<Color> colors = new List<Color>();
				List<Center> sortedCenters = new List<Center>(island.centers);
				sortedCenters.Sort((a, b) => {
					bool xLess = a.coord.x < b.coord.x;
					return xLess || (xLess && a.coord.y < b.coord.y) ? -1 : 1;
				});
				foreach (Center center in sortedCenters) {
					addTrianglesForCenter(center, indices, vertices, colors, verticalScale);
					if (vertices.Count > vertexLimit) {
						addMeshSubObject(gameObject, material, world.size, indices, vertices, colors);
					}
				}
				if (vertices.Count > 0) {
					addMeshSubObject(gameObject, material, world.size,  indices, vertices, colors);
				}
			}
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
            Vector3 vertexCenter = new Vector3((float)center.coord.x, center.scaledElevation(verticalScale), (float)center.coord.y);
            Color color = getColor(center.terrainType);
			if (center.isClipped) {
				return;
			}

            List<Corner> corners = center.corners;
            for (int i = 0; i < corners.Count; i++) {
				Corner corner1 = corners[i];
				int index2 = i + 1 >= corners.Count ? 0 : i + 1;
				Corner corner2 = corners[index2];

				Vector3 vertex1 = new Vector3((float)corner1.coord.x, corner1.scaledElevation(verticalScale), (float)corner1.coord.y);
				Vector3 vertex2 = new Vector3((float)corner2.coord.x, corner2.scaledElevation(verticalScale), (float)corner2.coord.y);
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
