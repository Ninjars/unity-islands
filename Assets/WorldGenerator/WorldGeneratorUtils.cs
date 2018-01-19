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
				centers.Add(new Center(i, new Coord(position.X, position.Y)));
			}

			return centers;
		}

        internal static List<Corner> createCorners(List<HalfEdge> halfEdges) {
            List<Corner> corners = new List<Corner>(halfEdges.Count);
			for (int i = 0; i < halfEdges.Count; ++i) {
				HalfEdge edge = halfEdges[i];
				var position = edge.Origin;
				corners.Add(new Corner(i, new Coord(position.X, position.Y)));
			}
			return corners;
        }

        internal static List<Edge> createEdges(List<IEdge> voronoiEdges, List<Center> centers, List<Corner> corners) {
            List<Edge> edges = new List<Edge>(voronoiEdges.Count);

			return edges;
        }
    }
}
