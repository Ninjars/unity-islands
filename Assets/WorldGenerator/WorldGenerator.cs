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
		public float waterClip = 0.25f;

		public bool debugDrawDelauney = false;
		public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = true;
		
        private World world;

        private void Start() {
            GameObject gameObj = new GameObject();
            gameObj.name = "Island";

            int seed = 12335;
            world = generateWorld(seed);

			WorldGenElevation.createIsland(world, waterClip);
			// world.islandRims = calculateIslandRims(world.corners);
			// WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));

			WorldGenMesh meshGenerator = new WorldGenMesh(world, gameObj, verticalScale, material);
			meshGenerator.triangulate();
        }

        private List<List<Corner>> calculateIslandRims(List<Corner> corners) {
            List<Corner> rimCorners = corners.Where(corner => corner.isIslandRim).ToList();
			List<List<Corner>> rims = new List<List<Corner>>();

			while (rimCorners.Count > 0) {
				Corner initial = rimCorners[0];
				List<Corner> rim = buildIslandRim(initial);
				rimCorners.RemoveAll(c => rim.Contains(c));
			}
			return rims;
		}

		private List<Corner> buildIslandRim(Corner initialCorner) {
			Corner prev = initialCorner;
			Corner current = getClockwiseRim(initialCorner);

			List<Corner> islandRim = new List<Corner>();
			islandRim.Add(initialCorner);
			islandRim.Add(current);
			while (true) {
				// assumption that each corner has two adjactent rim corners; breaks if we have an awkward geometry meeting
				// TODO: handle island point contacts
				Corner next = getNextRimCorner(current, prev);
				if (next == initialCorner) {
					// completed island circumference
					return islandRim;

				} else {
					islandRim.Add(next);
					prev = current;
					current = next;
				}
			}
		}

        private Corner getClockwiseRim(Corner initialCorner) {
            List<Corner> rimCorners = initialCorner.GetAdjacents().Where(corner => corner.isIslandRim).ToList();
            List<Corner> rimLand = initialCorner.GetAdjacents().Where(corner => !corner.isClipped).ToList();

			Debug.Log("getClockwiseRim " + rimCorners.Count);
			foreach (Corner corner in rimCorners) {
				Debug.Log(corner.coord.x + " " + corner.coord.y);
			}
			Debug.Assert(rimCorners.Count == 2);

			Corner rimA = rimCorners[0];
			Corner rimB = rimCorners[1];
			Vector2 o = new Vector2((float) initialCorner.coord.x, (float) initialCorner.coord.y);
			Vector2 a = new Vector2((float) rimA.coord.x, (float) rimA.coord.y);
			Vector2 b = new Vector2((float) rimB.coord.x, (float) rimB.coord.y);
			Vector2 c = new Vector2((float) rimLand[0].coord.x, (float) rimLand[0].coord.y);

			Vector2 oa = o - a;
			Vector2 ob = o - b;
			Vector2 oc = o - c;

			double angleOA = Math.Atan2(oa.y, oa.x);
			double angleOB = Math.Atan2(ob.y, ob.x);
			double angleOC = Math.Atan2(oc.y, oc.x);

			if (angleOA > angleOC && angleOC > angleOB) {
				return rimA;
			} else {
				return rimB;
			}
        }

        private Corner getNextRimCorner(Corner current, Corner previous) {
				List<Corner> adjacentRims = current.GetAdjacents().Where(corner => corner.isIslandRim).ToList();
				Debug.Assert(adjacentRims.Count == 2);
				if (adjacentRims[0] == previous) {
					return adjacentRims[1];
				} else {
					return adjacentRims[0];
				}
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

        private World generateWorld(int seed) {
			VoronoiBase voronoi = WorldGeneratorUtils.generateVoronoi(seed, worldSize, pointCount, initialDistributionCurve);

			List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices, worldSize);

			List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

			List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

			WorldGeneratorUtils.recenterCorners(corners);

			return new World(seed, worldSize, centers, corners, edges);
        }
    }
}
