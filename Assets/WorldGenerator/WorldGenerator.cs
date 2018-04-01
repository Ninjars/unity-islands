using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Voronoi;
using System;
using TriangleNet.Topology.DCEL;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using System.Linq;

namespace WorldGenerator {
    public class WorldGenerator : MonoBehaviour {

		public Material material;
		public AnimationCurve initialDistributionCurve;
		public float worldSize = 1000;
		public int pointCount = 1000;
		public float verticalScale = 100f;

		[Range(0, 1f)]
		public float clippingHeight = 0.25f;

		public bool debugDrawDelauney = false;
		public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = true;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";

            int seed = 12335;
            Graph graph = generateGraph(seed);

			WorldGenElevation.generateElevations(graph, clippingHeight);
			WorldGenElevation.applyClipping(graph, clippingHeight);
			List<Island> islands = findIslands(graph);

			world = new World(seed, worldSize, graph, islands);

			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
			WorldGenMesh.triangulate(gameObj, material, world, worldSize);
        }

        private List<Island> findIslands(Graph graph) {
            List<Center> landCenters = graph.centers.Where(center => !center.isClipped).ToList();
			List<Island> islands = new List<Island>();
			while (landCenters.Count > 0) {
				List<Center> islandCenters = new List<Center>();
				Center startCenter = landCenters[0];
				islandCenters.Add(startCenter);
				List<Center> queue = startCenter.neighbours.Where(center => !center.isClipped).ToList();
				while (queue.Count > 0) {
					Center next = queue[0];
					queue.Remove(next);
					foreach (Center neigh in next.neighbours) {
						if (!neigh.isClipped && !islandCenters.Contains(neigh)) {
							islandCenters.Add(neigh);
							queue.Add(neigh);
						}
					}
				}
				landCenters.RemoveAll(c => islandCenters.Contains(c));
				islands.Add(new Island(islandCenters));
			}
			return islands;
        }

        void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			if (debugDrawDelauney) {
				foreach (Center center in world.centers) {
					foreach (Center neigh in center.neighbours) {
						if (neigh.index > center.index) {
							Debug.DrawLine(
								new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
								new Vector3((float) neigh.coord.x, neigh.scaledElevation(verticalScale), (float) neigh.coord.y));
						}
					}
				}
			}
			if (debugDrawDownlopes) {
				foreach (Center center in world.centers) {
					if (center.downslope != null) {
						Debug.DrawLine(
							new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
							new Vector3((float) center.downslope.coord.x, center.downslope.scaledElevation(verticalScale)+1, (float) center.downslope.coord.y),
							Color.red);
					}				
				}
			}
			if (debugDrawCornerConnections) {
				foreach (Corner corner in world.corners) {
					foreach (Center center in corner.GetTouches()) {
						Debug.DrawLine(
								new Vector3((float) center.coord.x, center.scaledElevation(verticalScale), (float) center.coord.y), 
								new Vector3((float) corner.coord.x, corner.scaledElevation(verticalScale), (float) corner.coord.y),
								Color.green);
					}
				}
			}
		}

        private Graph generateGraph(int seed) {
			VoronoiBase voronoi = WorldGeneratorUtils.generateVoronoi(seed, worldSize, pointCount, initialDistributionCurve);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices, worldSize);

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			WorldGeneratorUtils.recenterCorners(corners);

			return new Graph(seed, worldSize, centers, corners, edges);
        }
    }
}
