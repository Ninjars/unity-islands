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

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
            MeshFilter meshFilter = gameObj.AddComponent<MeshFilter>();
            gameObj.AddComponent<MeshRenderer>();

            int seed = 12335;
			List<Vector2> initialPoints = generateInitialPoints(seed);
            World world = generateWorldGeometry(initialPoints);

			WorldGeneratorUtils.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
			
			foreach (Vector2 point in initialPoints) {
            	GameObject debugIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				debugIndicator.name = "point " + point;
				debugIndicator.transform.position = new Vector3(point.x, 0, point.y);
				debugIndicator.transform.localScale = new Vector3(10, 10, 10);
			}
			foreach (Center center in world.centers) {
            	GameObject debugIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				debugIndicator.name = "center " + center.index;
				debugIndicator.transform.position = new Vector3((float) center.coord.x, 5, (float) center.coord.y);
				debugIndicator.transform.localScale = new Vector3(10, 10, 10);
				// debugIndicator.GetComponent<MeshRenderer>().material.color = center.isBorder ? Color.black : Color.gray;
				debugIndicator.GetComponent<MeshRenderer>().material.color = Color.black;
			}
			foreach (Corner corner in world.corners) {
            	GameObject debugIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
				debugIndicator.name = "corner " + corner.index;
				debugIndicator.transform.position = new Vector3((float) corner.coord.x, 5, (float) corner.coord.y);
				debugIndicator.transform.localScale = new Vector3(10, 10, 10);
				// debugIndicator.GetComponent<MeshRenderer>().material.color = corner.isBorder ? Color.black : Color.gray;
				debugIndicator.GetComponent<MeshRenderer>().material.color = Color.gray;
			}

            Mesh mesh = meshFilter.mesh;
			assignMeshVertices(world, mesh);
			// assignVertexColors(world, mesh);

            gameObj.AddComponent<MeshCollider>().sharedMesh = mesh;
			gameObj.GetComponent<Renderer>().material = material;
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

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			// WorldGeneratorUtils.improveCorners(corners);

			return new World(centers, corners, edges);
        }

		private void assignMeshVertices(World world, Mesh mesh) {
			List<int> indices = new List<int>();
			List<Vector3> positions = new List<Vector3>();
			// Triangulator.triangulateVoronoi(voronoi, out indices, out positions);

			Polygon poly = new Polygon();
			Dictionary<int, TriangleNet.Geometry.Vertex> vertices = new Dictionary<int, TriangleNet.Geometry.Vertex>(world.corners.Count);
			foreach (Corner corner in world.corners) {
				var vertex = new TriangleNet.Geometry.Vertex(corner.coord.x, corner.coord.y);
				vertices.Add(corner.index, vertex);
				poly.Add(vertex);
			}

			IMesh triangulated = poly.Triangulate();
			foreach (TriangleNet.Geometry.Vertex vert in triangulated.Vertices) {
				Corner corner = findCorner(world.corners, vert.X, vert.Y);
				if (corner != null) {
					corner.addVertexIndex(positions.Count);
				}
				positions.Add(new Vector3((float) vert.X, 0, (float) vert.Y));
			}
			foreach (Triangle tri in triangulated.Triangles) {
				indices.Add(tri.GetVertexID(0));
				indices.Add(tri.GetVertexID(1));
				indices.Add(tri.GetVertexID(2));
			}

			mesh.Clear();
			mesh.vertices = positions.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.RecalculateNormals();
			flipMeshNormals (mesh);

			Vector2[] myUVs = new Vector2[positions.Count];
			for (var i = 0; i < positions.Count; i++) {
				myUVs[i] = new Vector2(positions[i].x / worldSize, positions[i].y / worldSize);
			}
			mesh.uv = myUVs;
		}

        private Corner findCorner(List<Corner> corners, double x, double y) {
            foreach (Corner corner in corners) {
				Coord coord = corner.coord;
				if (coord.matches(x, y)) {
					return corner;
				}
			}
			return null;
        }

        private void assignVertexColors(World world, Mesh mesh) {
			Color[] colors = new Color[mesh.vertices.Length];
			foreach (Corner corner in world.corners) {
				foreach (int index in corner.vertexIndices) {
					colors[index] = getColorFromCorner(corner);
				}
			}
			mesh.colors = colors;
		}

		private Color getColorFromCorner(Corner corner) {
			switch (corner.terraintype) {
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

        //private Corner findCorner(List<Corner> corners, int i) {
        //    foreach (Corner corner in corners) {
		//		if (corner.vertexIndices.Contains(i)) {
		//			return corner;
		//		}
		//	}
		//	return null;
        //}

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
