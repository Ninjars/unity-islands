using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Voronoi;
using System;
using TriangleNet.Topology.DCEL;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;

namespace WorldGenerator {
    public class WorldGenerator : MonoBehaviour {

		public Material material;
		public AnimationCurve initialDistributionCurve;
		internal const float worldSize = 1000;
		private const int pointCount = 100;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
            MeshFilter meshFilter = gameObj.AddComponent<MeshFilter>();
            gameObj.AddComponent<MeshRenderer>();

            int seed = 12335;
            world = generateWorldGeometry(seed);

			WorldGenElevation.createVolcanicIsland(world);
			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));

            Mesh mesh = meshFilter.mesh;
			triangulate(world, mesh);

            gameObj.AddComponent<MeshCollider>().sharedMesh = mesh;
			gameObj.GetComponent<Renderer>().material = material;
        }

		void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			foreach (Center center in world.centers) {
				foreach (Center neigh in center.neighbours) {
					if (neigh.index > center.index) {
						Debug.DrawLine(
							new Vector3((float) center.coord.x, (float) center.elevation + 1, (float) center.coord.y), 
							new Vector3((float) neigh.coord.x, (float) neigh.elevation + 1, (float) neigh.coord.y));
					}
				}
			}
		}

        private World generateWorldGeometry(int seed) {
			VoronoiBase voronoi = WorldGeneratorUtils.generateVoronoi(seed, worldSize, pointCount, initialDistributionCurve);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices);

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			return new World(seed, worldSize, centers, corners, edges);
        }

		private void triangulate(World world, Mesh mesh) {
			List<int> indices = new List<int>();
			List<Vector3> positions = new List<Vector3>();
			List<Color> colors = new List<Color>();
			foreach (Center center in world.centers) {
				addTrianglesForCenter(center, indices, positions, colors);
			}
			
			mesh.Clear();
			mesh.vertices = positions.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.RecalculateNormals();
			Vector2[] myUVs = new Vector2[positions.Count];
			for (var i = 0; i < positions.Count; i++) {
				myUVs[i] = new Vector2(positions[i].x / worldSize, positions[i].y / worldSize);
			}
			mesh.uv = myUVs;
		}

        private void addTrianglesForCenter(Center center, List<int> indices, List<Vector3> positions, List<Color> colors) {
            int indicesOffset = positions.Count;
			
            Vector3 centerPos = new Vector3((float)center.coord.x, 0, (float)center.coord.y);
            positions.Add(centerPos);
            Color color = getColor(center.terrainType);
            colors.Add(color);

            List<Corner> corners = center.corners;
            for (int i = 1; i < corners.Count; i++) {
				Corner corner = corners[i];
                positions.Add(new Vector3((float)corner.coord.x, 0, (float)corner.coord.y));
				colors.Add(color);
                indices.Add(indicesOffset);
                indices.Add(indicesOffset + i);
                indices.Add(indicesOffset + i + 1);
            }
            positions.Add(new Vector3((float)corners[0].coord.x, 0, (float)corners[0].coord.y));
			colors.Add(color);
            indices.Add(indicesOffset);
            indices.Add(indicesOffset + corners.Count);
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

        private void flipMeshNormals(Mesh mesh) {
			var indices = mesh.triangles;
			var triangleCount = indices.Length / 3;
			for (var i = 0; i < triangleCount; i++) {
				var tmp = indices [i * 3];
				indices [i * 3] = indices [i * 3 + 1];
				indices [i * 3 + 1] = tmp;
			}
			mesh.triangles = indices;
			// additionally flip the vertex normals to get the correct lighting
			var normals = mesh.normals;
			for (var n = 0; n < normals.Length; n++) {
				normals [n] = -normals [n];
			}
			mesh.normals = normals;
		}
    }
}
