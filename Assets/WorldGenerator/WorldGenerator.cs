using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Voronoi;
using System;
using TriangleNet.Topology.DCEL;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using System.Linq;

namespace WorldGenerator {
    public class WorldGenerator : MonoBehaviour {
		private const int vertexLimit = 63000;

		public Material material;
		public AnimationCurve initialDistributionCurve;
		public float worldSize = 1000;
		public int pointCount = 1000;
		public float verticalScale = 100f;

		[Range(0, 1f)]
		public float waterClip = 0.25f;

		public bool debugDrawDelauney = false;
		public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = true;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";

            int seed = 12335;
            world = generateWorldGeometry(seed);

			WorldGenElevation.createIsland(world, waterClip);
			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));

			triangulate(world, gameObj);
        }

		void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			if (debugDrawDelauney) {
				foreach (Center center in world.centers) {
					foreach (Center neigh in center.neighbours) {
						if (neigh.index > center.index) {
							Debug.DrawLine(
								new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
								new Vector3((float) neigh.coord.x, neigh.scaledElevation(verticalScale), (float) neigh.coord.y));
						}
					}
				}
			}
			if (debugDrawDownlopes) {
				foreach (Center center in world.centers) {
					if (center.downslope != null) {
						Debug.DrawLine(
							new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
							new Vector3((float) center.downslope.coord.x, center.downslope.scaledElevation(verticalScale)+1, (float) center.downslope.coord.y),
							Color.red);
					}				
				}
			}
			if (debugDrawCornerConnections) {
				foreach (Corner corner in world.corners) {
					foreach (Center center in corner.GetTouches()) {
						Debug.DrawLine(
								new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
								new Vector3((float) corner.coord.x, corner.scaledElevation(verticalScale), (float) corner.coord.y),
								Color.green);
					}
				}
			}
		}

        private World generateWorldGeometry(int seed) {
			VoronoiBase voronoi = WorldGeneratorUtils.generateVoronoi(seed, worldSize, pointCount, initialDistributionCurve);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices, worldSize);

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			WorldGeneratorUtils.recenterCorners(corners);

			return new World(seed, worldSize, centers, corners, edges);
        }

		private void triangulate(World world, GameObject islandGameObject) {
			List<int> indices = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			List<Center> sortedCenters = new List<Center>(world.centers);
			sortedCenters.Sort((a, b) => {
				bool xLess = a.coord.x < b.coord.x;
				return xLess || (xLess && a.coord.y < b.coord.y) ? -1 : 1;
			});
			foreach (Center center in sortedCenters) {
				addTrianglesForCenter(center, indices, vertices, colors);
				if (vertices.Count > vertexLimit) {
					addMeshSubObject(islandGameObject, indices, vertices, colors);
				}
			}
			if (vertices.Count > 0) {
				addMeshSubObject(islandGameObject, indices, vertices, colors);
			}
		}

		private void buildPedestal(World world, GameObject islandGameObject) {
			List<int> indices = new List<int>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();
			foreach (Center center in world.centers) {
				if (center.isOnRim) {
					if (vertices.Count > vertexLimit) {
						addMeshSubObject(islandGameObject, indices, vertices, colors);
					}
				}
			}
			if (vertices.Count > 0) {
				addMeshSubObject(islandGameObject, indices, vertices, colors);
			}
		}

        private void addMeshSubObject(GameObject containingObject, List<int> indices, List<Vector3> vertices, List<Color> colors) {
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
				myUVs[i] = new Vector2(vertices[i].x / worldSize, vertices[i].y / worldSize);
			}
			mesh.uv = myUVs;
			
			indices.Clear();
			vertices.Clear();
			colors.Clear();
		}

        private void addTrianglesForCenter(Center center, 
											List<int> indices, 
											List<Vector3> vertices,
											List<Color> colors) {
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

				if (corner1.isClipped || corner2.isClipped) {
					continue;
				} else {
					Vector3 vertex1 = new Vector3((float)corner1.coord.x, corner1.scaledElevation(verticalScale), (float)corner1.coord.y);
					Vector3 vertex2 = new Vector3((float)corner2.coord.x, corner2.scaledElevation(verticalScale), (float)corner2.coord.y);
					addTriangle(vertexCenter, vertex1, vertex2, color, indices, vertices, colors);
				}
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

		private Color getColor(TerrainType terrainType) {
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
