﻿using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;
using UnityEngine;

namespace WorldGenerator {
	public static class WorldGeneratorUtils {

		public static VoronoiBase generateVoronoi(int seed, float worldSize, int pointCount, AnimationCurve curve) {
			System.Random pointRandom = new System.Random(seed);
			List<Vector2> cornerPoints = new List<Vector2>(4);
			cornerPoints.Add(new Vector2(0, 0));
			cornerPoints.Add(new Vector2(worldSize, 0));
			cornerPoints.Add(new Vector2(0, worldSize));
			cornerPoints.Add(new Vector2(worldSize, worldSize));

			List<Vector2> points = new List<Vector2>(pointCount);
			points.AddRange(cornerPoints);
			for (int i = 0; i < pointCount; i++) {
				float x = curveWeightedRandom(curve, (float) pointRandom.NextDouble()) * worldSize;
				float y = curveWeightedRandom(curve, (float) pointRandom.NextDouble()) * worldSize;
                points.Add(new Vector2(x, y));
            }
			points = performLloydRelaxation(points, cornerPoints);
			points = performLloydRelaxation(points, cornerPoints);
			VoronoiBase voronoi = Triangulator.generateVoronoi(points);
			voronoi.ResolveBoundaryEdges();
			return voronoi;
		}

		private static float curveWeightedRandom(AnimationCurve curve, float value) {
			return curve.Evaluate(value);
		}

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
				Center c = new Center(face.ID, new Coord(face.GetPoint().X, face.GetPoint().Y));
				centers.Add(c);
				foreach (HalfEdge halfEdge in face.EnumerateEdges()) {
					c.AddCorner(corners[halfEdge.Origin.ID]);
				}
			}
			return centers;
		}

        internal static List<Corner> createCorners(List<TriangleNet.Topology.DCEL.Vertex> vertices) {
            List<Corner> corners = new List<Corner>(vertices.Count);
			foreach (TriangleNet.Topology.DCEL.Vertex vertex in vertices) {
				corners.Add(new Corner(vertex.ID, new Coord(vertex.X, vertex.Y)));
			}
			return corners;
        }

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
    }
}
