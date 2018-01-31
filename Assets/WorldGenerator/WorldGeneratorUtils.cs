using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;
using UnityEngine;

namespace WorldGenerator {
	public static class WorldGeneratorUtils {

		private const float LAKE_THRESHOLD = 0.3f;

        internal static void separateTheLandFromTheWater(World world, PerlinIslandShape perlinIslandShape) {
			// assign coarse water/land separation to corners
            foreach (Corner corner in world.corners) {
				bool isWater = !isInsideShape(perlinIslandShape, corner.coord);
				if (corner.isBorder) {
					corner.terraintype = TerrainType.OCEAN;
				} else if (isWater) {
					corner.terraintype = TerrainType.LAKE;
				} else {
					corner.terraintype = TerrainType.LAND;
				}
			}

			// assign coarse water/land separation to centers
			List<Center> borderCenters = new List<Center>();
			foreach (Center center in world.centers) {
				int waterCornerCount = 0;
				foreach (Corner corner in center.corners) {
					if (corner.isBorder) {
						center.terrainType = TerrainType.OCEAN;
						borderCenters.Add(center);
						continue;
					}
					if (corner.isWater()) {
						waterCornerCount++;
					}
				}
				if (center.terrainType != TerrainType.OCEAN
						&& waterCornerCount >= center.corners.Count * LAKE_THRESHOLD) {
					center.terrainType = TerrainType.LAKE;
				}
			}

			// flood fill center's ocean property
			int i = 0;
			while (i < borderCenters.Count) {
				Center c = borderCenters[i];
				foreach (Center other in c.neighbours) {
					if (other.terrainType == TerrainType.LAKE) {
						other.terrainType = TerrainType.OCEAN;
						borderCenters.Add(other);
					}
				}
				i++;
			}

			// TODO: coast and shallows
        }

		private static bool isInsideShape(PerlinIslandShape shape, Coord coordinate) {
			return shape.isInside((float) coordinate.x, (float) coordinate.y);
		}

        internal static List<Center> createCenters(List<Face> faces) {
			List<Center> centers = new List<Center>(faces.Count);
			foreach (Face face in faces) {
				Center c = new Center(face.ID, new Coord(face.GetPoint().X, face.GetPoint().Y));
				centers.Add(c);
			}
			return centers;
		}

        internal static List<Corner> createCorners(List<TriangleNet.Topology.DCEL.Vertex> vertices) {
            List<Corner> corners = new List<Corner>(vertices.Count);
			foreach (TriangleNet.Topology.DCEL.Vertex vertex in vertices) {
				corners.Add(new Corner(vertex.ID, new Coord(vertex.X, vertex.Y)));
				Debug.Log("creating corner " + vertex.ID);
			}
			return corners;
        }

        internal static List<Edge> createEdges(VoronoiBase voronoi, List<Center> centers, List<Corner> corners) {
			List<IEdge> voronoiEdges = voronoi.Edges;
            List<Edge> edges = new List<Edge>(voronoiEdges.Count);
			List<HalfEdge> halfEdges = voronoi.HalfEdges;
			
			foreach (IEdge voronoiEdge in voronoiEdges) {
				HalfEdge e0 = halfEdges[voronoiEdge.P0];
				HalfEdge e1 = halfEdges[voronoiEdge.P1];
				Center center0 = centers[e0.Face.ID];
				Center center1 = centers[e1.Face.ID];
				Debug.Assert(center0 != null);
				Debug.Assert(center1 != null);

				TriangleNet.Topology.DCEL.Vertex v0 = e0.Origin;
				TriangleNet.Topology.DCEL.Vertex v1 = e1.Origin;
				Debug.Log("v0: " + v0.ID);
				Debug.Log("v1: " + v1.ID);

				Corner corner0 = corners[v0.ID];
				Corner corner1 = corners[v1.ID];
				Debug.Assert(corner0 != null);
				Debug.Assert(corner1 != null);
				bool isBorder = e0 == null || e1 == null;
				Debug.Log("is border " + center0.index + " " + center1.index + ": " + isBorder);
				edges.Add(makeEdge(isBorder, corner0, corner1, center0, center1));
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
				center0.AddCorner(corner0);
				center0.AddCorner(corner1);
				center0.AddEdge(edge);
			}
			if (center1 != null) {
				center1.AddCorner(corner0);
				center1.AddCorner(corner1);
				center1.AddEdge(edge);
			}

			if (center0 != null && isBorder) {
				center0.isBorder = isBorder;
			}
			if (center1 != null && isBorder) {
				center1.isBorder = isBorder;
			}
			return edge;
		}
    }
}
