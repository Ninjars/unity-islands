using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using UnityEngine;

namespace WorldGenerator {
	public static class WorldGeneratorUtils {
        internal static List<Center> createCenters(List<Face> faces) {
			List<Center> centers = new List<Center>(faces.Count);

			for (int i = 0; i < faces.Count; ++i) {
				Face face = faces[i];
				var position = face.GetPoint();
				double x = 0;
				double y = 0;
				int count = 0;
				foreach (var corner in face.EnumerateEdges()) {
					x += corner.Origin.X;
					y += corner.Origin.Y;
					count++;
				}
				x /= count;
				y /= count;
				centers.Add(new Center(i, new Coord(x, y)));
			}

			return centers;
		}

        internal static List<Corner> createCorners(List<TriangleNet.Topology.DCEL.Vertex> vertices) {
            List<Corner> corners = new List<Corner>(vertices.Count);
			for (int i = 0; i < vertices.Count; ++i) {
				TriangleNet.Topology.DCEL.Vertex vertex = vertices[i];
				corners.Add(new Corner(vertex.ID, new Coord(vertex.X, vertex.Y)));
			}
			return corners;
        }

        internal static List<Edge> createEdges(List<IEdge> voronoiEdges, List<TriangleNet.Topology.DCEL.Vertex> vertices, 
												List<Center> centers, List<Corner> corners, List<Face> faces) {
            List<Edge> edges = new List<Edge>(voronoiEdges.Count);

			for (int i = 0; i < voronoiEdges.Count; ++i) {
				IEdge voronoiEdge = voronoiEdges[i];

				var neighbouringCenters = getCentersByEdge(voronoiEdge, centers, faces);
				Center center0 = neighbouringCenters.Count > 0 ? neighbouringCenters[0] : null;
				Center center1 = neighbouringCenters.Count > 1 ? neighbouringCenters[1] : null;

				TriangleNet.Topology.DCEL.Vertex v0 = vertices[voronoiEdge.P0];
				TriangleNet.Topology.DCEL.Vertex v1 = vertices[voronoiEdge.P1];
				Corner corner0 = null;
				Corner corner1 = null;
				foreach (Corner corner in corners) {
					if (corner0 == null && corner.matchesId(v0.ID)) {
						corner0 = corner;
					} else if (corner1 == null && corner.matchesId(v1.ID)) {
						corner1 = corner;
					}
					if (corner0 != null && corner1 != null) {
						break;
					}
				}
				Debug.Assert(corner0 != null);
				Debug.Assert(corner1 != null);
				bool isBorder = neighbouringCenters.Count < 2;
				edges.Add(makeEdge(i, isBorder, corner0, corner1, center0, center1));
			}
			return edges;
        }

        internal static void improveCorners(List<Corner> corners) {
			foreach (Corner corner in corners) {
				double x = 0;
				double y = 0;

				foreach (Center center in corner.GetTouches()) {
					x += center.coord.x;
					y += center.coord.y;
				}

				x /= corner.GetTouches().Count;
				y /= corner.GetTouches().Count;
				corner.setPosition(x, y);
			}
        }

        private static Edge makeEdge(int index, bool isBorder, Corner corner0, Corner corner1, Center center0, Center center1) {
			Edge edge = new Edge(index, isBorder, corner0, corner1, center0, center1);
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

			if (center0 != null && isBorder) {
				center0.isBorder = isBorder;
			}
			if (center1 != null && isBorder) {
				center1.isBorder = isBorder;
			}
			return edge;
		}

        private static List<Center> getCentersByEdge(IEdge edge, List<Center> centers, List<Face> faces) {
			List<Center> found = new List<Center>(2);
			foreach (Center center in centers) {
				Face face = faces[center.index];
				foreach (HalfEdge corner in face.EnumerateEdges()) {
					if (corner.ID.Equals(edge.P0) || corner.ID.Equals(edge.P1)) {
						found.Add(center);
						break;
					}
				}
			}
			return found;
		}
    }
}
