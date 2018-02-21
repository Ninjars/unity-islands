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
		internal const float worldSize = 1000;
		private const int pointCount = 100;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
            MeshFilter meshFilter = gameObj.AddComponent<MeshFilter>();
            gameObj.AddComponent<MeshRenderer>();

            int seed = 12335;
			List<Vector2> initialPoints = generateInitialPoints(seed);
            world = generateWorldGeometry(initialPoints);

			WorldGeneratorUtils.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
			
			foreach (Center center in world.centers) {
            	GameObject debugIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				debugIndicator.name = "center " + center.index;
				debugIndicator.transform.position = new Vector3((float) center.coord.x, 5, (float) center.coord.y);
				debugIndicator.transform.localScale = new Vector3(5, 5, 5);
				debugIndicator.GetComponent<MeshRenderer>().material.color = Color.black;
			}

            Mesh mesh = meshFilter.mesh;
			triangulate(world, mesh);
			// assignVertexColors(world, mesh);

            gameObj.AddComponent<MeshCollider>().sharedMesh = mesh;
			gameObj.GetComponent<Renderer>().material = material;
        }

		void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			foreach (Center center in world.centers) {
				foreach (Center neigh in center.neighbours) {
					Debug.DrawLine(new Vector3((float) center.coord.x, 5, (float) center.coord.y), new Vector3((float) neigh.coord.x, 3, (float) neigh.coord.y));
				}
			}
		}

		private List<Vector2> generateInitialPoints(int seed) {
			List<Vector2> initialPoints = new List<Vector2>(pointCount);
			initialPoints.Add(new Vector2(0, 0));
			initialPoints.Add(new Vector2(worldSize, 0));
			initialPoints.Add(new Vector2(0, worldSize));
			initialPoints.Add(new Vector2(worldSize, worldSize));
            System.Random pointRandom = new System.Random(seed);
            for (int i = 0; i < pointCount; i++) {
                initialPoints.Add(new Vector2((float)pointRandom.NextDouble() * worldSize, (float)pointRandom.NextDouble() * worldSize));
            }
			return initialPoints;
		}

        private World generateWorldGeometry(List<Vector2> initialPoints) {
			VoronoiBase voronoi = Triangulator.generateVoronoi(initialPoints);
			voronoi.ResolveBoundaryEdges();

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices);

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			return new World(centers, corners, edges);
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
			flipMeshNormals (mesh);
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
			colors.Add(getColor(center.terrainType));

            List<Corner> corners = center.corners;
            for (int i = 1; i < corners.Count; i++) {
				Corner corner = corners[i];
                positions.Add(new Vector3((float)corner.coord.x, 0, (float)corner.coord.y));
				colors.Add(getColor(corner.terrainType));
                indices.Add(indicesOffset);
                indices.Add(indicesOffset + i);
                indices.Add(indicesOffset + i + 1);
            }
            positions.Add(new Vector3((float)corners[0].coord.x, 0, (float)corners[0].coord.y));
			colors.Add(getColor(corners[0].terrainType));
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
				default:
					return Color.magenta;
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
