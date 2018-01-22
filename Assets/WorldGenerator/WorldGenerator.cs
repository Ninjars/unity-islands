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

		private const float worldSize = 1000;
		private const int pointCount = 100;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
            MeshFilter meshFilter = gameObj.AddComponent<MeshFilter>();
            gameObj.AddComponent<MeshRenderer>();

            Mesh mesh = meshFilter.mesh;
            generateWorld(mesh);

            gameObj.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void generateWorld(Mesh mesh) {
            int seed = 12335;
            
            List<Vector2> initialPoints = new List<Vector2>(pointCount);
			initialPoints.Add(new Vector2(0, 0));
			initialPoints.Add(new Vector2(worldSize, 0));
			initialPoints.Add(new Vector2(0, worldSize));
			initialPoints.Add(new Vector2(worldSize, worldSize));
            System.Random pointRandom = new System.Random(seed);
            for (int i = 0; i < pointCount; i++) {
                initialPoints.Add(new Vector2((float)pointRandom.NextDouble() * worldSize, (float)pointRandom.NextDouble() * worldSize));
            }

			VoronoiBase voronoi = Triangulator.generateVoronoi(initialPoints);
			voronoi.ResolveBoundaryEdges();

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi.Edges, voronoi.Vertices, centers, corners, voronoi.Faces);

			WorldGeneratorUtils.improveCorners(corners);

			assignMeshVertices(corners, edges, mesh);
        }

		private void assignMeshVertices(List<Corner> corners, List<Edge> edges, Mesh mesh) {
			List<int> indices = new List<int>();
			List<Vector3> positions = new List<Vector3>();
			// Triangulator.triangulateVoronoi(voronoi, out indices, out positions);

			Polygon poly = new Polygon();
			Dictionary<int, TriangleNet.Geometry.Vertex> vertices = new Dictionary<int, TriangleNet.Geometry.Vertex>(corners.Count);
			foreach (Corner corner in corners) {
				var vertex = new TriangleNet.Geometry.Vertex(corner.coord.x, corner.coord.y);
				vertices.Add(corner.index, vertex);
				poly.Add(vertex);
			}

			IMesh triangulated = poly.Triangulate();
			foreach (TriangleNet.Geometry.Vertex vert in triangulated.Vertices) {
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
