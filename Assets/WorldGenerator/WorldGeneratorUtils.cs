using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;
using UnityEngine;

namespace WorldGenerator {
	public static class WorldGeneratorUtils {

		public static VoronoiBase generateVoronoi(int seed, float worldSize, int pointCount) {
			System.Random pointRandom = new System.Random(seed);
			List<Vector2> cornerPoints = new List<Vector2>(4);
			cornerPoints.Add(new Vector2(0, 0));
			cornerPoints.Add(new Vector2(worldSize, 0));
			cornerPoints.Add(new Vector2(0, worldSize));
			cornerPoints.Add(new Vector2(worldSize, worldSize));

			List<Vector2> points = new List<Vector2>(pointCount);
			points.AddRange(cornerPoints);
			for (int i = 0; i < pointCount; i++) {
				float x = (float) pointRandom.NextDouble() * worldSize;
				float y = (float) pointRandom.NextDouble() * worldSize;
                points.Add(new Vector2(x, y));
            }
			points = performLloydRelaxation(points, cornerPoints);
			points = performLloydRelaxation(points, cornerPoints);
			VoronoiBase voronoi = Triangulator.generateVoronoi(points);
			voronoi.ResolveBoundaryEdges();
			return voronoi;
		}

		/**
			Reposition the current center points to be at the average position of their corners.
			Doing this makes the point distribution more even, setting it up to create a better mesh.
			Can be performed recursively, each iteration yielding smaller and smaller improvements.
		*/
        private static List<Vector2> performLloydRelaxation(List<Vector2> currentPoints, List<Vector2> cornerPoints) {
			VoronoiBase voronoi = Triangulator.generateVoronoi(currentPoints);
			List<TriangleNet.Topology.DCEL.Face> faces = voronoi.Faces;
			List<Vector2> points = new List<Vector2>(currentPoints.Count + cornerPoints.Count);
			points.AddRange(cornerPoints);
			foreach (Face face in faces) {
				float x = 0, y = 0;
                IEnumerable<HalfEdge> halfEdges = face.EnumerateEdges();
				float count = 0;
                foreach (var halfEdge in halfEdges) {
					count++;
					x += (float) halfEdge.Origin.X;
					y += (float) halfEdge.Origin.Y;
				}
				points.Add(new Vector2(x / count, y / count));
			}
            return points;
        }

        internal static List<Center> createCenters(List<Face> faces, List<Corner> corners) {
			List<Center> centers = new List<Center>(faces.Count);
			foreach (Face face in faces) {
				Center c = new Center(face.ID, new Coord((float) face.GetPoint().X, 0, (float) face.GetPoint().Y));
				centers.Add(c);
				foreach (HalfEdge halfEdge in face.EnumerateEdges()) {
					c.AddCorner(corners[halfEdge.Origin.ID]);
				}
			}
			return centers;
		}

        internal static List<Corner> createCorners(List<TriangleNet.Topology.DCEL.Vertex> vertices) {
			List<Corner> idCorners = new List<Corner>(vertices.Count);
			for (int i = 0; i < vertices.Count; i++) {
				TriangleNet.Topology.DCEL.Vertex vertex = vertices[i];
				idCorners.Add(new Corner(new Coord((float) vertex.X, 0, (float) vertex.Y)));
			}
			return idCorners;
        }

		/**
			Iterate over every edge in the voronoi graph building edge objects and connecting centers and corners.
			This method is very important to complete the graph to be used for all later stages!
		*/
        internal static List<Edge> createEdges(VoronoiBase voronoi, List<Center> centers, List<Corner> corners) {
            List<Edge> edges = new List<Edge>();
			List<HalfEdge> halfEdges = voronoi.HalfEdges;

			foreach (HalfEdge e0 in halfEdges) {
				HalfEdge e1 = e0.Twin;
				if (e1.ID < e0.ID) {
					continue;
				}

				TriangleNet.Topology.DCEL.Vertex v0 = e0.Origin;
				TriangleNet.Topology.DCEL.Vertex v1 = e1.Origin;

				Corner corner0 = corners[v0.ID];
				Corner corner1 = corners[v1.ID];

				Face face0 = e0.Face;
				Face face1 = e1.Face;
				Center center0 = face0.ID < 0 ? null : centers[face0.ID];
				Center center1 = face1.ID < 0 ? null : centers[face1.ID];
				bool isBorder = center0 == null || center1 == null;
				edges.Add(makeEdge(isBorder, corner0, corner1, center0, center1));
			}
			return edges;
        }

        private static Edge makeEdge(bool isBorder, Corner corner0, Corner corner1, Center center0, Center center1) {
			Edge edge = new Edge(isBorder, corner0, corner1, center0, center1);
			if (center0 != null && center1 != null) {
				center0.AddNeighbour(center1);
				center1.AddNeighbour(center0);
			}
			corner0.AddEdge(edge);
			corner0.AddCenter(center0);
			corner0.AddCenter(center1);
			corner0.AddAdjacent(corner1);
			corner0.isBorder = isBorder;

			corner1.AddEdge(edge);
			corner1.AddCenter(center0);
			corner1.AddCenter(center1);
			corner1.AddAdjacent(corner0);
			corner1.isBorder = isBorder;

			if (center0 != null) {
				center0.AddEdge(edge);
                center0.isBorder = isBorder;
            }
			if (center1 != null) {
				center1.AddEdge(edge);
                center1.isBorder = isBorder;
            }
			return edge;
		}

		/**
			Call after connecting corners with centers in order to convert the voronoi graph into barycentric dual mesh.
			This positions the corner at the center point of the centers it touches, leading to a much more uniform mesh.
		*/
		internal static void recenterCorners(List<Corner> corners) {
			foreach (Corner corner in corners) {
				float x = 0, y = 0;
				List<Center> centers = corner.GetTouches();
				foreach (Center center in centers) {
					x += (float) center.coord.x;
					y += (float) center.coord.y;
				}
				corner.coord.setXY(x / centers.Count, y / centers.Count); 
			}
		}
    }
}
