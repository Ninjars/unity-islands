using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

		public int worldSeed = 12345;

		public List<GameObject> agentsToGenerateNavMeshFor;

		[Range(0, 1f)]
		public float clippingHeight = 0.25f;
		
        private World world;

		public World generateWorld() {
			DebugLoggingTimer timer = new DebugLoggingTimer();
			timer.begin("generateWorld");
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
			Rigidbody rigidbody = gameObj.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

            Graph graph = generateGraph(worldSeed);

			timer.logEventComplete("generateGraph()");

			WorldGenElevation.generateElevations(graph, clippingHeight);
			timer.logEventComplete("generateElevations()");

			List<Island> islands = findIslands(graph);
			Debug.Log("island count " + islands.Count);
			timer.logEventComplete("findIslands()");

			world = new World(worldSeed, worldSize, verticalScale, graph, islands);
			timer.logEventComplete("instantiateWorld()");

			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
			
			foreach (Island island in world.islands) {
				WorldGenMesh.triangulate(gameObj, material, island.centers, world.size, verticalScale);
				timer.logEventComplete("triangulate island " + island.GetHashCode());
				
				WorldGenElevation.generateIslandUndersideElevations(world.seed, island);
				timer.logEventComplete("generate underside for island " + island.GetHashCode());

				WorldGenMesh.triangulate(gameObj, material, island.undersideCoords, world.size, verticalScale, island.maxElevation - island.minElevation);
				timer.logEventComplete("triangulate underside for island " + island.GetHashCode());
			}

			// first pass at adding a nav mesh
			foreach (GameObject agentType in agentsToGenerateNavMeshFor) {
				NavMeshSurface navMeshSurface = gameObj.AddComponent<NavMeshSurface>();
				navMeshSurface.agentTypeID = agentType.GetComponent<NavMeshAgent>().agentTypeID;
				navMeshSurface.overrideVoxelSize = true;
				navMeshSurface.voxelSize = 0.5f;
				navMeshSurface.BuildNavMesh();
			}
			timer.logEventComplete("build NavMeshes");
			timer.end();
			return world;
		}

        private List<Vector3> getPointsForIslandUndersideGraph(Island island) {
            List<Vector3> points = island.centers.Select(center => new Vector3((float) center.coord.x, 0, (float) center.coord.y)).ToList();
			points.AddRange(island.corners.Where(corner => corner.isIslandRim).Select(corner => new Vector3((float) corner.coord.x, (float) corner.coord.y, (float) corner.coord.y)));
			return points;
        }

        private List<Island> findIslands(Graph graph) {
			DebugLoggingTimer timer = new DebugLoggingTimer();
			timer.begin("findIslands");
            List<Center> landCenters = graph.centers.Where(center => !center.isClipped).ToList();
			List<Island> islands = new List<Island>();
			timer.logEventComplete("perpare landCenters");
			while (landCenters.Count > 0) {
				Center startCenter = landCenters[0];
				List<Center> islandCenters = new List<Center>();
				islandCenters.Add(startCenter);
				List<Center> queue = startCenter.neighbours.Where(center => !center.isClipped).ToList();
				timer.logEventComplete("perpare queue");
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
				timer.logEventComplete("queue complete");
				foreach (var center in islandCenters) {
					landCenters.Remove(center);
				}
				if (islandCenters.Count > 1) {
					// filter out single-center islands for being too small
					islands.Add(createIsland(islandCenters));
				}
				timer.logEventComplete("createIsland island");
			}
			timer.end();
			return islands;
        }

		private Island createIsland(List<Center> islandCenters) {
			float minX = float.MaxValue;
			float minY = float.MaxValue;
			float maxX = float.MinValue;
			float maxY = float.MinValue;
			foreach (Center center in islandCenters) {
				Coord coord = center.coord;
				if (coord.x < minX) {
					minX = coord.x;
				}
				if (coord.x > maxX) {
					maxX = coord.x;
				}
				if (coord.y < minY) {
					minY = coord.y;
				}
				if (coord.y > maxY) {
					maxY = coord.y;
				}
			}
			Rect rect = new Rect((float) minX, (float) minY, (float) (maxX - minX), (float) ( maxY - minY));
			Vector3 islandCenter = new Vector3(rect.center.x, 0, rect.center.y);
			return new Island(islandCenters, islandCenter, rect);
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
