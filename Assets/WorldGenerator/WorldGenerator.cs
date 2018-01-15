using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Voronoi;
using System;
using TriangleNet.Topology.DCEL;

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

			VoronoiBase voronoi = Triangulator.generateVoronoi (initialPoints);
			voronoi.ResolveBoundaryEdges();

			List<Center> centers = createCenters(voronoi.Faces, worldSize);

			assignMeshVertices (voronoi, mesh);
        }

        private List<Center> createCenters(List<Face> faces, float worldSize) {
			List<Center> centers = new List<Center>(faces.Count);

			for (int i = 0; i < faces.Count; ++i) {
				Face face = faces[i];
				var position = face.GetPoint();
				centers.Add(new Center(i, new Coord(position.X, position.Y)));
			}

			return centers;
		}

		private void assignMeshVertices(VoronoiBase voronoi, Mesh mesh) {
			List<int> indices;
			List<Vector3> positions;
			Triangulator.triangulateVoronoi(voronoi, out indices, out positions);

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
