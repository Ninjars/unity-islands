using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TriangleNet.Geometry;
using UnityEngine;
using TriangleNet.Voronoi;
using TriangleNet.Meshing;
using TriangleNet.Topology.DCEL;

namespace WorldGenerator {
	
	public class Triangulator {

		public static VoronoiBase generateVoronoi(List<Vector2> points) {
			var poly = new Polygon (points.Count());
			foreach (Vector2 point in points) {
				poly.Add (new TriangleNet.Geometry.Vertex (point.x, point.y));
			}
			TriangleNet.Mesh mesh = (TriangleNet.Mesh) poly.Triangulate (new ConstraintOptions() { ConformingDelaunay = true});
			return new BoundedVoronoi (mesh);
		}

		public static void triangulateVoronoi(VoronoiBase voronoi, out List<int> outIndices, out List<Vector3> outVertices) {
			outIndices = new List<int>();
            outVertices = new List<Vector3>();
			foreach (Face face in voronoi.Faces) {
				int offset = outVertices.Count;
				HalfEdge[] edges = face.EnumerateEdges().ToArray();
				edges.Reverse ();
				for(int i = 1; i < edges.Count()-1; i++) {
					outIndices.Add(offset);
					outIndices.Add(offset + i);
					outIndices.Add(offset + i + 1);
				}
				outVertices.AddRange ((from edge in edges select new Vector3 ((float)edge.Origin.X, 0f, (float)edge.Origin.Y)).ToList());
            }
		}
    }
}
