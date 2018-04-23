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

		public List<GameObject> agentsToGenerateNavMeshFor;

		[Range(0, 1f)]
		public float clippingHeight = 0.25f;

		public bool debugDrawDelauney = false;
		public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = true;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";
			Rigidbody rigidbody = gameObj.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;

            int seed = 12335;
            Graph graph = generateGraph(seed);

			WorldGenElevation.generateElevations(graph, clippingHeight);
			WorldGenElevation.applyClipping(graph, clippingHeight);
			List<Island> islands = findIslands(graph);
			Debug.Log("island count " + islands.Count);

			world = new World(seed, worldSize, graph, islands);

			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
			
			foreach (Island island in world.islands) {
				WorldGenMesh.triangulate(gameObj, material, island.centers, world.size, verticalScale);
				
				WorldGenElevation.generateIslandUndersideElevations(world.seed, island);
				WorldGenMesh.triangulate(gameObj, material, island.undersideCoords, world.size, verticalScale, island.maxElevation - island.minElevation);
			}

			// first pass at adding a nav mesh
			foreach (GameObject agentType in agentsToGenerateNavMeshFor) {
				NavMeshSurface navMeshSurface = gameObj.AddComponent<NavMeshSurface>();
				navMeshSurface.agentTypeID = agentType.GetComponent<NavMeshAgent>().agentTypeID;
				navMeshSurface.BuildNavMesh();
			}
        }

        private List<Vector3> getPointsForIslandUndersideGraph(Island island) {
            List<Vector3> points = island.centers.Select(center => new Vector3((float) center.coord.x, 0, (float) center.coord.y)).ToList();
			points.AddRange(island.corners.Where(corner => corner.isIslandRim).Select(corner => new Vector3((float) corner.coord.x, (float) corner.coord.y, (float) corner.coord.y)));
			return points;
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
				islands.Add(createIsland(islandCenters));
			}
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

        void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			if (debugDrawDelauney) {
				foreach (Center center in world.centers) {
					foreach (Center neigh in center.neighbours) {
						if (neigh.index > center.index) {
							Debug.DrawLine(
								center.coord.toVector3(verticalScale * 1.01f), 
								neigh.coord.toVector3(verticalScale * 1.01f));
						}
					}
				}
			}
			if (debugDrawDownlopes) {
				foreach (Corner corner in world.corners) {
					if (corner.downslope != null) {
						Debug.DrawLine(
								corner.coord.toVector3(verticalScale * 1.01f), 
								corner.downslope.coord.toVector3(verticalScale * 1.01f),
							Color.red);
					}				
				}
			}
			if (debugDrawCornerConnections) {
				foreach (Corner corner in world.corners) {
					foreach (Center center in corner.GetTouches()) {
						Debug.DrawLine(
								center.coord.toVector3(verticalScale * 1.01f), 
								corner.coord.toVector3(verticalScale * 1.01f),
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
